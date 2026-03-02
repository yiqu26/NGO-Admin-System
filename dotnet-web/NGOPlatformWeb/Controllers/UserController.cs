using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGOPlatformWeb.Models.Entity;
using NGOPlatformWeb.Models.ViewModels;
using NGOPlatformWeb.Models.ViewModels.Profile;
using NGOPlatformWeb.Models.ViewModels.ActivityRegistrations;
using NGOPlatformWeb.Models.ViewModels.Purchase;
using NGOPlatformWeb.Services;
using System.Security.Claims;

namespace NGOPlatformWeb.Controllers
{
    public class UserController : BaseController
    {
        private readonly PasswordService _passwordService;
        private readonly AchievementService _achievementService;

        public UserController(NGODbContext context, PasswordService passwordService, ImageUploadService imageUploadService, AchievementService achievementService)
            : base(context, imageUploadService)
        {
            _passwordService = passwordService;
            _achievementService = achievementService;
        }
        // 一般使用者的個人資料頁面 - 顯示基本資料、活動參與統計、認購統計
        [Authorize(Roles = "User")]
        public async Task<IActionResult> UserProfile()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToLogin();

            var (activityStats, purchaseStats, achievements) = await GetUserStatisticsAsync(user.UserId);

            var vm = new UserProfileViewModel
            {
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                IdentityNumber = user.IdentityNumber,
                Password = user.Password,
                ProfileImage = user.ProfileImage ?? _imageUploadService.GetDefaultProfileImage("user"),
                
                TotalActivitiesRegistered = activityStats.Total,
                ActiveRegistrations = activityStats.Active,
                RecentActivities = activityStats.Recent,
                
                TotalPurchaseOrders = purchaseStats.Total,
                TotalPurchaseAmount = purchaseStats.Amount,
                RecentPurchases = purchaseStats.Recent,
                
                Achievements = achievements
            };

            return View(vm);
        }

        // 提取統計資料計算邏輯
        private async Task<(ActivityStats activity, PurchaseStats purchase, List<UserAchievementViewModel> achievements)> GetUserStatisticsAsync(int userId)
        {
            var activityRegistrations = await _context.UserActivityRegistrations
                .Include(r => r.Activity)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.RegisterTime)
                .Take(5)
                .ToListAsync();

            var purchaseOrders = await _context.UserOrders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            var activityStats = new ActivityStats
            {
                Total = await _context.UserActivityRegistrations.Where(r => r.UserId == userId).CountAsync(),
                Active = await _context.UserActivityRegistrations.Where(r => r.UserId == userId && r.Status == "registered").CountAsync(),
                Recent = activityRegistrations.Select(r => new ActivitySummary
                {
                    ActivityId = r.ActivityId,
                    ActivityName = r.Activity?.ActivityName ?? "未知活動",
                    StartDate = r.Activity?.StartDate ?? DateTime.MinValue,
                    Status = r.Status,
                    ImageUrl = r.Activity?.ImageUrl ?? "/images/activity-default.png"
                }).ToList()
            };

            var purchaseStats = new PurchaseStats
            {
                Total = await _context.UserOrders.Where(o => o.UserId == userId).CountAsync(),
                Amount = await _context.UserOrders.Where(o => o.UserId == userId && o.PaymentStatus == "已付款").SumAsync(o => o.TotalPrice),
                Recent = purchaseOrders.Select(o => new PurchaseSummary
                {
                    OrderId = o.UserOrderId,
                    OrderDate = o.OrderDate,
                    TotalPrice = o.TotalPrice,
                    Status = o.PaymentStatus,
                    OrderNumber = o.OrderNumber
                }).ToList()
            };

            List<UserAchievementViewModel> achievements = new();
            try
            {
                var newAchievements = await _achievementService.CheckAndAwardAchievements(userId);
                achievements = await _achievementService.GetUserAchievements(userId);
                
                if (newAchievements.Any())
                    TempData["NewlyEarnedAchievements"] = string.Join(",", newAchievements);
            }
            catch { }

            return (activityStats, purchaseStats, achievements);
        }

        // 內部類別用於統計資料
        private class ActivityStats
        {
            public int Total { get; set; }
            public int Active { get; set; }
            public List<ActivitySummary> Recent { get; set; } = new();
        }

        private class PurchaseStats
        {
            public int Total { get; set; }
            public decimal Amount { get; set; }
            public List<PurchaseSummary> Recent { get; set; } = new();
        }

        // 使用者編輯個人資料頁面 - GET 方法
        [Authorize(Roles = "User")]
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToLogin();

            var vm = new UserEditViewModel
            {
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                IdentityNumber = user.IdentityNumber,
                ProfileImage = user.ProfileImage ?? _imageUploadService.GetDefaultProfileImage("user")
            };

            return View(vm);
        }

        // 使用者編輯個人資料頁面 - POST 方法
        [Authorize(Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> EditProfile(UserEditViewModel vm, IFormFile? profileImageFile)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToLogin();

            // 處理圖片上傳
            string? newImagePath = user.ProfileImage;
            if (profileImageFile != null)
            {
                var uploadResult = await _imageUploadService.UploadImageAsync(profileImageFile, user.ProfileImage);
                if (!uploadResult.Success)
                {
                    ModelState.AddModelError("ProfileImage", uploadResult.ErrorMessage ?? "圖片上傳失敗");
                    vm.ProfileImage = user.ProfileImage ?? _imageUploadService.GetDefaultProfileImage("user");
                    return View(vm);
                }
                newImagePath = uploadResult.ImagePath;
            }

            // 更新使用者資料
            user.Name = vm.Name;
            user.Phone = vm.Phone;
            user.IdentityNumber = vm.IdentityNumber;
            user.ProfileImage = newImagePath;
            
            // 只有當用戶輸入新密碼時才更新密碼
            if (!string.IsNullOrWhiteSpace(vm.NewPassword))
            {
                user.Password = _passwordService.HashPassword(vm.NewPassword);
            }

            _context.SaveChanges();

            // 重新登入以更新 Cookie 中的資料
            await HttpContext.SignOutAsync();
            await AuthController.SignInAsync(HttpContext,
                id: user.UserId.ToString(),
                name: user.Name ?? "使用者",
                role: "User",
                email: user.Email ?? "");

            return RedirectToAction("UserProfile");
        }

        // AJAX 頭像上傳 API
        [Authorize(Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> UploadProfileImage(IFormFile profileImage)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Json(new { success = false, message = "找不到使用者資料" });

            return await HandleProfileImageUpload(profileImage, imagePath => 
            {
                user.ProfileImage = imagePath;
                return Task.CompletedTask;
            });
        }


        // 使用者活動報名紀錄頁面 - 顯示所有活動參與歷史
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Registrations()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToLogin();

            var registrations = await _context.UserActivityRegistrations
                .Include(r => r.Activity)
                .Where(r => r.UserId == user.UserId)
                .OrderByDescending(r => r.RegisterTime)
                .ToListAsync();

            var viewModel = new UserActivityRegistrationsViewModel
            {
                UserName = user.Name ?? "訪客",
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

        // 使用者認購紀錄頁面 - 顯示所有物資捐贈歷史
        [Authorize(Roles = "User")]
        public async Task<IActionResult> PurchaseRecords()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToLogin();

            // 取得該使用者的所有認購紀錄（包含訂單詳情、物資資訊和緊急物資認購記錄）
            var orders = await _context.UserOrders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Supply)
                .Where(o => o.UserId == user.UserId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
            
            // 取得緊急物資認購記錄（包含緊急需求的圖片資訊）
            var emergencyPurchases = await _context.EmergencyPurchaseRecords
                .Where(ep => orders.Select(o => o.UserOrderId).Contains(ep.UserOrderId))
                .ToListAsync();
                
            // 取得相關的緊急需求資料（用於取得圖片）
            var emergencyNeedIds = emergencyPurchases.Select(ep => ep.EmergencyNeedId).Distinct().ToList();
            var emergencyNeeds = emergencyNeedIds.Any() ? await _context.EmergencySupplyNeeds
                .Where(en => emergencyNeedIds.Contains(en.EmergencyNeedId))
                .ToListAsync() : new List<EmergencySupplyNeeds>();
            

            // 建立認購紀錄的 ViewModel
            var viewModel = new UserPurchaseRecordsViewModel
            {
                UserName = user.Name ?? "訪客",
                Orders = orders.Select(o => new OrderRecordViewModel
                {
                    OrderId = o.UserOrderId,
                    OrderNumber = o.OrderNumber,
                    OrderDate = o.OrderDate,
                    TotalPrice = o.TotalPrice,
                    PaymentStatus = o.PaymentStatus,
                    PaymentMethod = o.PaymentMethod,
                    OrderSource = o.OrderSource,
                    EmergencyNeedId = o.EmergencyNeedId,
                    Items = o.OrderSource == "emergency" ? 
                        // 緊急物資訂單 - 從緊急物資認購記錄取得資料
                        emergencyPurchases.Where(ep => ep.UserOrderId == o.UserOrderId)
                            .Select(ep => {
                                var emergencyNeed = emergencyNeeds.FirstOrDefault(en => en.EmergencyNeedId == ep.EmergencyNeedId);
                                var imageUrl = "/images/user-default.png"; // 預設圖片
                                
                                // 如果緊急需求有圖片且不為空，使用該圖片
                                if (emergencyNeed != null && !string.IsNullOrEmpty(emergencyNeed.ImageUrl))
                                {
                                    imageUrl = emergencyNeed.ImageUrl;
                                }
                                else
                                {
                                    // 根據物資名稱智能匹配圖片
                                    imageUrl = GetEmergencyImageByName(ep.SupplyName);
                                }
                                
                                return new OrderItemViewModel
                                {
                                    SupplyName = ep.SupplyName,
                                    Quantity = ep.Quantity,
                                    UnitPrice = ep.UnitPrice,
                                    TotalPrice = ep.UnitPrice * ep.Quantity,
                                    ImageUrl = imageUrl,
                                    IsEmergency = true,
                                    OrderSource = "emergency"
                                };
                            }).ToList() :
                        // 一般物資訂單 - 從訂單明細取得資料
                        o.OrderDetails.Select(od => new OrderItemViewModel
                        {
                            SupplyName = od.Supply?.SupplyName ?? "未知物資",
                            Quantity = od.Quantity,
                            UnitPrice = od.UnitPrice,
                            TotalPrice = od.UnitPrice * od.Quantity,
                            ImageUrl = od.Supply?.ImageUrl ?? "/images/default-supply.png",
                            IsEmergency = false,
                            OrderSource = od.OrderSource
                        }).ToList()
                }).ToList()
            };

            return View(viewModel);
        }

        // 根據物資名稱智能匹配圖片
        private static string GetEmergencyImageByName(string supplyName)
        {
            var name = supplyName?.ToLower() ?? "";
            
            // 根據已有的圖片資源匹配
            if (name.Contains("胰島素") || name.Contains("藥") || name.Contains("醫療"))
                return "/images/saline.jpg"; // 使用生理食鹽水圖片代表醫療用品
            else if (name.Contains("急救包") || name.Contains("醫療急救"))
                return "/images/bandage.png"; // 使用繃帶圖片代表急救包
            else if (name.Contains("紙尿褲") || name.Contains("尿布"))
                return "/images/wipes.jpg"; // 使用濕紙巾圖片代表個人護理用品
            else if (name.Contains("罐頭") || name.Contains("食物"))
                return "/images/corn.png"; // 使用玉米罐頭圖片代表食物
            else if (name.Contains("睡袋") || name.Contains("衣"))
                return "/images/coat.png"; // 使用外套圖片代表衣物
            else
                return "/images/user-default.png"; // 預設圖片
        }
    }
}