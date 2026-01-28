// 移除 System.Drawing 依賴，改用檔案擴展名驗證

namespace NGOPlatformWeb.Services
{
    public class ImageUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly long _maxFileSize = 2 * 1024 * 1024; // 2MB (業界標準)
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png" }; // 只支援最常用格式
        private readonly string _uploadFolder = "images/profiles";

        public ImageUploadService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<(bool Success, string? ImagePath, string? ErrorMessage)> UploadImageAsync(IFormFile file, string? oldImagePath = null)
        {
            try
            {
                // 驗證檔案
                var validationResult = ValidateFile(file);
                if (!validationResult.IsValid)
                {
                    return (false, null, validationResult.ErrorMessage);
                }

                // 創建上傳目錄
                var uploadPath = Path.Combine(_environment.WebRootPath, _uploadFolder);
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // 生成唯一檔名
                var fileName = GenerateUniqueFileName(file);
                var filePath = Path.Combine(uploadPath, fileName);

                // 儲存檔案
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // 如果有舊圖片，刪除它
                if (!string.IsNullOrEmpty(oldImagePath))
                {
                    DeleteOldImage(oldImagePath);
                }

                // 返回相對路徑
                var relativePath = $"/{_uploadFolder}/{fileName}";
                return (true, relativePath, null);
            }
            catch (Exception ex)
            {
                return (false, null, $"圖片上傳失敗：{ex.Message}");
            }
        }

        private (bool IsValid, string? ErrorMessage) ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return (false, "請選擇要上傳的圖片");
            }

            if (file.Length > _maxFileSize)
            {
                return (false, "檔案大小不能超過 2MB");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                return (false, "只允許上傳 JPG、PNG 格式的圖片");
            }

            return (true, null);
        }

        private string GenerateUniqueFileName(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var uniqueId = Guid.NewGuid().ToString("N");
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            return $"profile_{timestamp}_{uniqueId}{extension}";
        }

        private void DeleteOldImage(string imagePath)
        {
            try
            {
                if (imagePath.StartsWith("/"))
                {
                    imagePath = imagePath.Substring(1);
                }

                var fullPath = Path.Combine(_environment.WebRootPath, imagePath);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch
            {
                // 忽略刪除舊檔案的錯誤
            }
        }

        public string GetDefaultProfileImage(string userType = "user")
        {
            return userType.ToLower() == "case" 
                ? "/images/case-avatar-circle.svg" 
                : "/images/user-default-new.svg";
        }
    }
}