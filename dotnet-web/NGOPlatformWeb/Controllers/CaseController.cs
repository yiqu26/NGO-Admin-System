using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NGOPlatformWeb.Models.ViewModels;
using NGOPlatformWeb.Models.ViewModels.Profile;
using NGOPlatformWeb.Models.ViewModels.ActivityRegistrations;
using NGOPlatformWeb.Services;
using System.Security.Claims;

namespace NGOPlatformWeb.Controllers
{
    [Authorize(Roles = "Case")]
    public class CaseController : Controller
    {
        private readonly ICaseService _caseService;
        private readonly PasswordService _passwordService;
        private readonly ImageUploadService _imageUploadService;

        public CaseController(ICaseService caseService, PasswordService passwordService, ImageUploadService imageUploadService)
        {
            _caseService = caseService;
            _passwordService = passwordService;
            _imageUploadService = imageUploadService;
        }

        public async Task<IActionResult> ShoppingIndex(string category)
        {
            var supplies = await _caseService.GetSuppliesAsync(category);
            return View(supplies);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplySupply(int supplyId, int quantity)
        {
            var caseIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(caseIdClaim))
                return RedirectToAction("Login", "Auth");

            int caseId = int.Parse(caseIdClaim);
            await _caseService.ApplySupplyAsync(caseId, supplyId, quantity);

            return RedirectToAction("CasePurchaseList");
        }

        public async Task<IActionResult> CasePurchaseList(string category)
        {
            var caseIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (caseIdClaim == null)
                return RedirectToAction("Login", "Auth");

            int caseId = int.Parse(caseIdClaim.Value);
            var viewModel = await _caseService.GetCasePurchaseListAsync(caseId);

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> CaseActivityList()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Auth");

            var (login, cas) = await _caseService.GetCaseByEmailAsync(email);
            if (login == null || cas == null)
                return RedirectToAction("Login", "Auth");

            var registrations = await _caseService.GetCaseActivityRegistrationsAsync(cas.CaseId);

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

        [Authorize(Roles = "Case")]
        public async Task<IActionResult> CaseProfile()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login", "Auth");

            var (login, cas) = await _caseService.GetCaseByEmailAsync(email);
            if (login == null || cas == null) return NotFound();

            var (total, active, recent) = await _caseService.GetCaseActivityCountsAsync(cas.CaseId);

            var vm = new CaseProfileViewModel
            {
                Name = cas.Name ?? "",
                Email = login.Email ?? "",
                Phone = cas.Phone ?? "",
                IdentityNumber = cas.IdentityNumber ?? "",
                ProfileImage = cas.ProfileImage ?? _imageUploadService.GetDefaultProfileImage("case"),
                Birthday = cas.Birthday,
                Address = cas.FullAddress,
                TotalActivitiesRegistered = total,
                ActiveRegistrations = active,
                RecentActivities = recent.Select(r => new CaseActivitySummary
                {
                    ActivityId = r.ActivityId,
                    ActivityName = r.Activity?.ActivityName ?? "未知活動",
                    StartDate = r.Activity?.StartDate ?? DateTime.MinValue,
                    Status = r.Status,
                    ImageUrl = r.Activity?.ImageUrl ?? "/images/activity-default.png",
                    Category = r.Activity?.Category ?? ""
                }).ToList(),
                TotalApplications = 0,
                PendingApplications = 0
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Case")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CaseProfile(CaseProfileViewModel vm, IFormFile? profileImageFile)
        {
            if (vm.NewPassword != vm.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "密碼與確認密碼不一致");
                return View(vm);
            }

            var email = User.FindFirstValue(ClaimTypes.Email);
            var (login, cas) = await _caseService.GetCaseByEmailAsync(email ?? "");
            if (login == null || cas == null) return NotFound();

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

            var passwordHash = _passwordService.HashPassword(vm.NewPassword ?? "");
            await _caseService.UpdateCaseProfileAsync(login, cas, passwordHash, newImagePath);

            ViewBag.SuccessMessage = "密碼修改成功";
            return View(vm);
        }

        [Authorize(Roles = "Case")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadCaseProfileImage(IFormFile profileImage)
        {
            try
            {
                if (profileImage == null)
                    return Json(new { success = false, message = "請選擇要上傳的圖片" });

                var email = User.FindFirstValue(ClaimTypes.Email);
                var (login, cas) = await _caseService.GetCaseByEmailAsync(email ?? "");
                if (login == null)
                    return Json(new { success = false, message = "找不到個案登入資料" });
                if (cas == null)
                    return Json(new { success = false, message = "找不到個案資料" });

                var uploadResult = await _imageUploadService.UploadImageAsync(profileImage, cas.ProfileImage);
                if (!uploadResult.Success)
                    return Json(new { success = false, message = uploadResult.ErrorMessage });

                await _caseService.UpdateCaseImageAsync(cas, uploadResult.ImagePath ?? "");

                return Json(new
                {
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

        [Authorize(Roles = "Case")]
        public async Task<IActionResult> Registrations()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Auth");

            var (login, cas) = await _caseService.GetCaseByEmailAsync(email);
            if (login == null || cas == null)
                return RedirectToAction("Login", "Auth");

            var registrations = await _caseService.GetCaseActivityRegistrationsAsync(cas.CaseId);

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
