using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGOPlatformWeb.Models.Entity;
using NGOPlatformWeb.Services;
using System.Security.Claims;

namespace NGOPlatformWeb.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly NGODbContext _context;
        protected readonly ImageUploadService _imageUploadService;

        protected BaseController(NGODbContext context, ImageUploadService imageUploadService)
        {
            _context = context;
            _imageUploadService = imageUploadService;
        }

        // 統一的使用者認證和取得方法
        protected async Task<User?> GetCurrentUserAsync()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return null;
            
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        // 統一的個案認證和取得方法
        protected async Task<(CaseLogin? caseLogin, Case? cas)> GetCurrentCaseAsync()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return (null, null);

            var caseLogin = await _context.CaseLogins.FirstOrDefaultAsync(c => c.Email == email);
            if (caseLogin == null) return (null, null);

            var cas = await _context.Cases.FirstOrDefaultAsync(c => c.CaseId == caseLogin.CaseId);
            return (caseLogin, cas);
        }

        // 統一的頭像上傳處理
        protected async Task<IActionResult> HandleProfileImageUpload(IFormFile profileImage, Func<string, Task> updateImagePath)
        {
            try
            {
                if (profileImage == null)
                    return Json(new { success = false, message = "請選擇要上傳的圖片" });

                var uploadResult = await _imageUploadService.UploadImageAsync(profileImage, null);
                if (!uploadResult.Success)
                    return Json(new { success = false, message = uploadResult.ErrorMessage });

                await updateImagePath(uploadResult.ImagePath);
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

        // 統一的重定向到登入頁面
        protected IActionResult RedirectToLogin() => RedirectToAction("Login", "Auth");
    }
}