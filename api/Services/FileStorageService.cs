using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;

namespace NGO_WebAPI_Backend.Services
{
    /// <summary>
    /// 檔案儲存服務介面
    /// </summary>
    public interface IFileStorageService
    {
        /// <summary>
        /// 儲存服務是否可用
        /// </summary>
        bool IsAvailable { get; }

        /// <summary>
        /// 上傳音檔並回傳可存取的 URL
        /// </summary>
        Task<string> UploadAudioAsync(IFormFile audioFile);

        /// <summary>
        /// 上傳個案圖片並回傳可存取的 URL
        /// </summary>
        Task<string> UploadImageAsync(IFormFile imageFile);
    }

    /// <summary>
    /// 本地檔案儲存服務
    /// </summary>
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<LocalFileStorageService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _basePath;
        private readonly string _audioFolder;
        private readonly string _imageFolder;
        private readonly string _baseUrl;

        public LocalFileStorageService(IConfiguration configuration, ILogger<LocalFileStorageService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _basePath = _configuration["FileStorage:Local:BasePath"] ?? "uploads";
            _audioFolder = _configuration["FileStorage:Local:AudioFolder"] ?? "case_audio";
            _imageFolder = _configuration["FileStorage:Local:ImageFolder"] ?? "case-profiles";
            _baseUrl = _configuration["FileStorage:Local:BaseUrl"] ?? "/uploads";
        }

        public bool IsAvailable => true;

        private string GetAbsoluteBaseUrl()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request != null)
                return $"{request.Scheme}://{request.Host}{_baseUrl}";
            return _baseUrl;
        }

        public async Task<string> UploadAudioAsync(IFormFile audioFile)
        {
            var fullDir = Path.Combine(Directory.GetCurrentDirectory(), _basePath, _audioFolder);
            Directory.CreateDirectory(fullDir);

            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var guid = Guid.NewGuid().ToString("N")[..8];
            var extension = Path.GetExtension(audioFile.FileName).ToLowerInvariant();
            var fileName = $"{timestamp}_{guid}{extension}";

            var filePath = Path.Combine(fullDir, fileName);
            _logger.LogInformation("儲存音檔到本地: {Path}", filePath);

            using var stream = new FileStream(filePath, FileMode.Create);
            await audioFile.CopyToAsync(stream);

            var url = $"{GetAbsoluteBaseUrl()}/{_audioFolder}/{fileName}";
            _logger.LogInformation("本地音檔 URL: {Url}", url);
            return url;
        }

        public async Task<string> UploadImageAsync(IFormFile imageFile)
        {
            var fullDir = Path.Combine(Directory.GetCurrentDirectory(), _basePath, _imageFolder);
            Directory.CreateDirectory(fullDir);

            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var guid = Guid.NewGuid().ToString("N")[..8];
            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            var fileName = $"{timestamp}_{guid}{extension}";

            var filePath = Path.Combine(fullDir, fileName);
            _logger.LogInformation("儲存圖片到本地: {Path}", filePath);

            using var stream = new FileStream(filePath, FileMode.Create);
            await imageFile.CopyToAsync(stream);

            var url = $"{GetAbsoluteBaseUrl()}/{_imageFolder}/{fileName}";
            _logger.LogInformation("本地圖片 URL: {Url}", url);
            return url;
        }
    }

    /// <summary>
    /// Azure Blob Storage 檔案儲存服務
    /// </summary>
    public class AzureBlobStorageService : IFileStorageService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AzureBlobStorageService> _logger;

        public AzureBlobStorageService(IConfiguration configuration, ILogger<AzureBlobStorageService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public bool IsAvailable
        {
            get
            {
                var connectionString = _configuration.GetConnectionString("AzureStorage");
                var containerName = _configuration["AzureStorage:ContainerName"];
                return !string.IsNullOrEmpty(connectionString) && !string.IsNullOrEmpty(containerName);
            }
        }

        public async Task<string> UploadAudioAsync(IFormFile audioFile)
        {
            var containerClient = GetContainerClient();
            var audioFolder = _configuration["AzureStorage:AudioFolder"] ?? "case_audio/";

            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var guid = Guid.NewGuid().ToString("N")[..8];
            var extension = Path.GetExtension(audioFile.FileName).ToLowerInvariant();
            var blobName = $"{audioFolder}{timestamp}_{guid}{extension}";

            return await UploadToBlobAsync(audioFile, blobName);
        }

        public async Task<string> UploadImageAsync(IFormFile imageFile)
        {
            var containerClient = GetContainerClient();
            var imageFolder = _configuration["AzureStorage:ImageFolder"] ?? "case-profiles/";

            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var guid = Guid.NewGuid().ToString("N")[..8];
            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            var blobName = $"{imageFolder}{timestamp}_{guid}{extension}";

            return await UploadToBlobAsync(imageFile, blobName);
        }

        private BlobContainerClient GetContainerClient()
        {
            var connectionString = _configuration.GetConnectionString("AzureStorage");
            var containerName = _configuration["AzureStorage:ContainerName"];
            if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(containerName))
                throw new InvalidOperationException("Azure Storage 設定未配置");
            var blobServiceClient = new BlobServiceClient(connectionString);
            return blobServiceClient.GetBlobContainerClient(containerName);
        }

        private async Task<string> UploadToBlobAsync(IFormFile file, string blobName)
        {
            var containerClient = GetContainerClient();
            await containerClient.CreateIfNotExistsAsync();
            var blobClient = containerClient.GetBlobClient(blobName);
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;
            await blobClient.UploadAsync(stream, overwrite: true);
            var url = blobClient.Uri.ToString();
            _logger.LogInformation("Azure Blob 上傳成功: {Url}", url);
            return url;
        }
    }
}
