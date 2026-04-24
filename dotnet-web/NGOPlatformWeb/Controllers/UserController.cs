using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NGOPlatformWeb.Models.Entity;
using NGOPlatformWeb.Models.ViewModels.Profile;
using NGOPlatformWeb.Models.ViewModels.ActivityRegistrations;
using NGOPlatformWeb.Services;
using System.Security.Claims;

namespace NGOPlatformWeb.Controllers
{
    public class UserController : BaseController
    {
        private readonly PasswordService _passwordService;
        private readonly AchievementService _achievementService;
        private readonly IUserService _userService;

        public UserController(
            NGODbContext context,
            PasswordService passwordService,
            ImageUploadService imageUploadService,
            AchievementService achievementService,
            IUserService userService)
            : base(context, imageUploadService)
        {
            _passwordService = passwordService;
            _achievementService = achievementService;
            _userService = userService;
        }

        [Authorize(Roles = "User")]
        public async Task<IActionResult> UserProfile()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToLogin();

            var stats = await _userService.GetUserStatsAsync(user.UserId);

            List<UserAchievementViewModel> achievements = new();
            try
            {
                var newAchievements = await _achievementService.CheckAndAwardAchievements(user.UserId);
                achievements = await _achievementService.GetUserAchievements(user.UserId);
                if (newAchievements.Any())
                    TempData["NewlyEarnedAchievements"] = string.Join(",", newAchievements);
            }
            catch { }

            var vm = new UserProfileViewModel
            {
                Name = user.Name ?? "",
                Email = user.Email ?? "",
                Phone = user.Phone ?? "",
                IdentityNumber = user.IdentityNumber ?? "",
                ProfileImage = user.ProfileImage ?? _imageUploadService.GetDefaultProfileImage("user"),
                TotalActivitiesRegistered = stats.TotalActivities,
                ActiveRegistrations = stats.ActiveActivities,
                RecentActivities = stats.RecentActivities,
                TotalPurchaseOrders = stats.TotalPurchases,
                TotalPurchaseAmount = stats.TotalPurchaseAmount,
                RecentPurchases = stats.RecentPurchases,
                Achievements = achievements
            };

            return View(vm);
        }

        [Authorize(Roles = "User")]
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToLogin();

            var vm = new UserEditViewModel
            {
                Name = user.Name ?? "",
                Email = user.Email ?? "",
                Phone = user.Phone ?? "",
                IdentityNumber = user.IdentityNumber ?? "",
                ProfileImage = user.ProfileImage ?? _imageUploadService.GetDefaultProfileImage("user")
            };

            return View(vm);
        }

        [Authorize(Roles = "User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(UserEditViewModel vm, IFormFile? profileImageFile)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToLogin();

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

            user.Name = vm.Name;
            user.Phone = vm.Phone;
            user.IdentityNumber = vm.IdentityNumber;
            user.ProfileImage = newImagePath;

            if (!string.IsNullOrWhiteSpace(vm.NewPassword))
                user.Password = _passwordService.HashPassword(vm.NewPassword);

            await _userService.SaveUserAsync(user);

            await HttpContext.SignOutAsync();
            await AuthController.SignInAsync(HttpContext,
                id: user.UserId.ToString(),
                name: user.Name ?? "使用者",
                role: "User",
                email: user.Email ?? "");

            return RedirectToAction("UserProfile");
        }

        [Authorize(Roles = "User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
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

        [Authorize(Roles = "User")]
        public async Task<IActionResult> Registrations()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToLogin();

            var registrations = await _userService.GetUserRegistrationsAsync(user.UserId);

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

        [Authorize(Roles = "User")]
        public async Task<IActionResult> PurchaseRecords()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToLogin();

            var viewModel = await _userService.GetPurchaseRecordsAsync(user.UserId);
            viewModel.UserName = user.Name ?? "訪客";

            return View(viewModel);
        }
    }
}
