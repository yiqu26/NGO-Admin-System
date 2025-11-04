using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGOPlatformWeb.Models.Entity;
using NGOPlatformWeb.Models.ViewModels;
using NGOPlatformWeb.Models.ViewModels.Profile;
using NGOPlatformWeb.Models.ViewModels.ActivityRegistrations;
using NGOPlatformWeb.Services;
using System.Security.Claims;
// 個案身份操作功能，例如查看適用活動或可領取物資

namespace NGOPlatformWeb.Controllers
{
    public class CaseController : Controller
    {
        //目的：讓 Controller 能透過 DbContext 從資料庫撈資料，給 View 顯示。
        private readonly NGODbContext _context;
        private readonly PasswordService _passwordService;
        private readonly ImageUploadService _imageUploadService;

        public CaseController(NGODbContext context, PasswordService passwordService, ImageUploadService imageUploadService)
        {
            _context = context;
            _passwordService = passwordService;
            _imageUploadService = imageUploadService;
        }

        public IActionResult ShoppingIndex(string category)
        {
            var query = _context.Supplies
                .Include(s => s.SupplyCategory)
                .AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(s => s.SupplyCategory != null && s.SupplyCategory.SupplyCategoryName.Contains(category));
            }

            var supplies = query.ToList();

            // 傳遞使用者身份資訊給 View
            ViewBag.IsAuthenticated = User.Identity?.IsAuthenticated ?? false;
            ViewBag.UserRole = User.FindFirstValue(ClaimTypes.Role);

            return View(supplies);
        }

        [HttpPost]
        public IActionResult ApplySupply(int supplyId, int quantity)
        {
            var caseIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(caseIdClaim))
            {
                return RedirectToAction("Login", "Auth");
            }

            int caseId = int.Parse(caseIdClaim);

            var need = new RegularSupplyNeeds
            {
                CaseId = caseId,
                SupplyId = supplyId,
                Quantity = quantity,
                ApplyDate = DateTime.Now,
                Status = "pending"
            };

            _context.RegularSuppliesNeeds.Add(need);
            _context.SaveChanges();

            return RedirectToAction("CasePurchaseList");
        }

        public IActionResult CasePurchaseList(string category)
        {
            var caseIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (caseIdClaim == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            int caseId = int.Parse(caseIdClaim.Value);

            // 🔹 撈出未領取
            var unreceived = _context.RegularSuppliesNeeds
                .Include(r => r.Supply)
                    .ThenInclude(s => s.SupplyCategory)
                .Where(r => r.CaseId == caseId && r.Status == "pending")
                .Select(r => new SupplyRecordItem
                {
                    Name = r.Supply.SupplyName,
                    Category = r.Supply.SupplyCategory.SupplyCategoryName,
                    Quantity = r.Quantity,
                    ApplyDate = r.ApplyDate,
                    PickupDate = r.PickupDate,
                    Status = r.Status,
                    ImageUrl = r.Supply.ImageUrl
                })
                .OrderByDescending(r => r.ApplyDate)
                .ToList();

            // 🔹 撈出已領取 + 訪談物資
            var received = _context.RegularSuppliesNeeds
                .Include(r => r.Supply)
                    .ThenInclude(s => s.SupplyCategory)
                .Where(r => r.CaseId == caseId && (r.Status == "collected"))
                .Select(r => new SupplyRecordItem
                {
                    Name = r.Supply.SupplyName,
                    Category = r.Supply.SupplyCategory.SupplyCategoryName,
                    Quantity = r.Quantity,
                    ApplyDate = r.ApplyDate,
                    PickupDate = r.PickupDate,
                    Status = r.Status,
                    ImageUrl = r.Supply.ImageUrl
                })
                .Union(
                    _context.EmergencySupplyNeeds
                        .Where(e => e.CaseId == caseId && e.Status == "Completed")
                        .Select(e => new SupplyRecordItem
                        {
                            Name = e.SupplyName,
                            Category = "緊急物資",
                            Quantity = e.Quantity,
                            ApplyDate = e.CreatedDate ?? DateTime.Now,
                            PickupDate = e.UpdatedDate ?? DateTime.Now,
                            Status = "訪談物資",
                            ImageUrl = e.ImageUrl ?? "/images/emergency-default.png"
                        })
                )
                .OrderByDescending(s => s.PickupDate)
                .ToList();

            // 🔹 合併全部，用來統計，但排除掉「訪談物資」
            var allSupplies = unreceived
                .Concat(received.Where(s => s.Status != "訪談物資"))
                .ToList();

            // 🔥 統計各類別的總數量
            var categoryStats = allSupplies
                .GroupBy(s => s.Category)
                .Select(g => new CategoryStat
                {
                    CategoryName = g.Key ?? "未分類",
                    TotalQuantity = g.Sum(x => x.Quantity),
                    ItemCount = g.Count()
                })
                .ToList();

            // 🔹 填入 ViewModel
            var viewModel = new SupplyRecordViewModel
            {
                UnreceivedSupplies = unreceived,
                ReceivedSupplies = received,
                // 如果有 EmergencySupplies 要另外撈的話可以加
                CategoryStats = categoryStats
            };

            return View(viewModel);
        }


        [HttpGet]
        // 個案活動清單頁面 - 顯示已報名的活動（使用rich UI pattern）
        public async Task<IActionResult> CaseActivityList()
        {
            // 取得當前登入個案的 Email
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Auth");
            }

            // 透過 Email 找到個案的登入資料和基本資料
            var caseLogin = await _context.CaseLogins.FirstOrDefaultAsync(c => c.Email == email);
            var cas = await _context.Cases.FirstOrDefaultAsync(c => c.CaseId == caseLogin.CaseId);
            if (cas == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // 取得該個案的所有活動報名紀錄（包含活動詳情）
            var registrations = await _context.CaseActivityRegistrations
                .Include(r => r.Activity)
                .Where(r => r.CaseId == cas.CaseId)
                .OrderByDescending(r => r.RegisterTime)
                .ToListAsync();

            // 建立個案活動報名紀錄的 ViewModel
            var viewModel = new CaseActivityRegistrationsViewModel
            {
                CaseName = cas.Name ?? "個案",
                TotalRegistrations = registrations.Count,
                ActiveRegistrations = registrations.Count(r => r.Status == "registered"),
                Registrations = registrations.Select(r => new ActivityRegistrationItem
                {
                    RegistrationId = r.RegistrationId,
                    ActivityId = r.ActivityId,
                    ActivityName = r.Activity?.ActivityName ?? "未知活動",
                    ActivityDescription = r.Activity?.Description ?? "",
                    Location = r.Activity?.Location ?? "",
                    StartDate = r.Activity?.StartDate ?? DateTime.MinValue,
                    EndDate = r.Activity?.EndDate ?? DateTime.MinValue,
                    RegisterTime = r.RegisterTime,
                    Status = r.Status,
                    ImageUrl = r.Activity?.ImageUrl ?? "/images/activity-default.png",
                    Category = r.Activity?.Category ?? "",
                    TargetAudience = r.Activity?.TargetAudience ?? ""
                }).ToList()
            };

            return View(viewModel);
        }

        // 個案個人資料頁面 - 顯示和編輯個案資料
        [Authorize(Roles = "Case")]
        public async Task<IActionResult> CaseProfile()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login", "Auth");

            var caseLogin = await _context.CaseLogins.FirstOrDefaultAsync(c => c.Email == email);
            var cas = await _context.Cases.FirstOrDefaultAsync(c => c.CaseId == caseLogin.CaseId);
            if (cas == null) return NotFound();

            // 取得活動報名統計資料
            var activityRegistrations = await _context.CaseActivityRegistrations
                .Include(r => r.Activity)
                .Where(r => r.CaseId == cas.CaseId)
                .OrderByDescending(r => r.RegisterTime)
                .Take(5) // 取最近5筆活動紀錄
                .ToListAsync();

            // 計算總活動參與次數
            var totalActivities = await _context.CaseActivityRegistrations
                .Where(r => r.CaseId == cas.CaseId)
                .CountAsync();

            // 計算進行中的活動報名數
            var activeRegistrations = await _context.CaseActivityRegistrations
                .Where(r => r.CaseId == cas.CaseId && r.Status == "registered")
                .CountAsync();

            var vm = new CaseProfileViewModel
            {
                Name = cas.Name,
                Email = caseLogin.Email,
                Phone = cas.Phone,
                IdentityNumber = cas.IdentityNumber,
                ProfileImage = cas.ProfileImage ?? _imageUploadService.GetDefaultProfileImage("case"),
                Birthday = cas.Birthday,
                Address = cas.FullAddress,

                // 活動統計
                TotalActivitiesRegistered = totalActivities,
                ActiveRegistrations = activeRegistrations,
                RecentActivities = activityRegistrations.Select(r => new CaseActivitySummary
                {
                    ActivityId = r.ActivityId,
                    ActivityName = r.Activity?.ActivityName ?? "未知活動",
                    StartDate = r.Activity?.StartDate ?? DateTime.MinValue,
                    Status = r.Status,
                    ImageUrl = r.Activity?.ImageUrl ?? "/images/activity-default.png",
                    Category = r.Activity?.Category ?? ""
                }).ToList(),

                // 物資申請統計（預留擴展功能）
                TotalApplications = 0,
                PendingApplications = 0
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Case")]
        public async Task<IActionResult> CaseProfile(CaseProfileViewModel vm, IFormFile? profileImageFile)
        {
            if (vm.NewPassword != vm.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "密碼與確認密碼不一致");
                return View(vm);
            }

            var email = User.FindFirstValue(ClaimTypes.Email);
            var caseLogin = await _context.CaseLogins.FirstOrDefaultAsync(c => c.Email == email);
            if (caseLogin == null) return NotFound();

            var cas = await _context.Cases.FirstOrDefaultAsync(c => c.CaseId == caseLogin.CaseId);
            if (cas == null) return NotFound();

            // 處理圖片上傳
            string? newImagePath = cas.ProfileImage;
            if (profileImageFile != null)
            {
                var uploadResult = await _imageUploadService.UploadImageAsync(profileImageFile, cas.ProfileImage);
                if (uploadResult.Success)
                {
                    newImagePath = uploadResult.ImagePath;
                }
                else
                {
                    ModelState.AddModelError("ProfileImage", uploadResult.ErrorMessage ?? "圖片上傳失敗");
                    vm.ProfileImage = cas.ProfileImage ?? _imageUploadService.GetDefaultProfileImage("case");
                    return View(vm);
                }
            }

            // 更新密碼和頭像
            caseLogin.Password = _passwordService.HashPassword(vm.NewPassword);
            cas.ProfileImage = newImagePath;
            await _context.SaveChangesAsync();

            ViewBag.SuccessMessage = "密碼修改成功";
            return View(vm);
        }

        // AJAX 頭像上傳 API (個案專用)
        [Authorize(Roles = "Case")]
        [HttpPost]
        public async Task<IActionResult> UploadCaseProfileImage(IFormFile profileImage)
        {
            try
            {
                if (profileImage == null)
                {
                    return Json(new { success = false, message = "請選擇要上傳的圖片" });
                }

                // 取得當前登入個案
                var email = User.FindFirstValue(ClaimTypes.Email);
                var caseLogin = await _context.CaseLogins.FirstOrDefaultAsync(c => c.Email == email);
                if (caseLogin == null)
                {
                    return Json(new { success = false, message = "找不到個案登入資料" });
                }

                var cas = await _context.Cases.FirstOrDefaultAsync(c => c.CaseId == caseLogin.CaseId);
                if (cas == null)
                {
                    return Json(new { success = false, message = "找不到個案資料" });
                }

                // 上傳圖片
                var uploadResult = await _imageUploadService.UploadImageAsync(profileImage, cas.ProfileImage);
                if (!uploadResult.Success)
                {
                    return Json(new { success = false, message = uploadResult.ErrorMessage });
                }

                // 更新資料庫
                cas.ProfileImage = uploadResult.ImagePath;
                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = "頭像更新成功",
                    imageUrl = uploadResult.ImagePath
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"上傳失敗：{ex.Message}" });
            }
        }

        // 個案活動報名紀錄頁面 - 顯示個案所有活動參與歷史
        [Authorize(Roles = "Case")]
        public async Task<IActionResult> Registrations()
        {
            // 取得當前登入個案的 Email
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Auth");
            }

            // 透過 Email 找到個案的登入資料和基本資料
            var caseLogin = await _context.CaseLogins.FirstOrDefaultAsync(c => c.Email == email);
            var cas = await _context.Cases.FirstOrDefaultAsync(c => c.CaseId == caseLogin.CaseId);
            if (cas == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // 取得該個案的所有活動報名紀錄（包含活動詳情）
            var registrations = await _context.CaseActivityRegistrations
                .Include(r => r.Activity)
                .Where(r => r.CaseId == cas.CaseId)
                .OrderByDescending(r => r.RegisterTime)
                .ToListAsync();

            // 建立個案活動報名紀錄的 ViewModel
            var viewModel = new CaseActivityRegistrationsViewModel
            {
                CaseName = cas.Name ?? "個案",
                TotalRegistrations = registrations.Count,
                ActiveRegistrations = registrations.Count(r => r.Status == "registered"),
                Registrations = registrations.Select(r => new ActivityRegistrationItem
                {
                    RegistrationId = r.RegistrationId,
                    ActivityId = r.ActivityId,
                    ActivityName = r.Activity?.ActivityName ?? "未知活動",
                    ActivityDescription = r.Activity?.Description ?? "",
                    Location = r.Activity?.Location ?? "",
                    StartDate = r.Activity?.StartDate ?? DateTime.MinValue,
                    EndDate = r.Activity?.EndDate ?? DateTime.MinValue,
                    RegisterTime = r.RegisterTime,
                    Status = r.Status,
                    ImageUrl = r.Activity?.ImageUrl ?? "/images/activity-default.png",
                    Category = r.Activity?.Category ?? "",
                    TargetAudience = r.Activity?.TargetAudience ?? ""
                }).ToList()
            };

            return View(viewModel);
        }

    }
}
