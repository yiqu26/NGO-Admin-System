using Azure.Storage.Blobs;

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
    }

    /// <summary>
    /// 本地檔案儲存服務
    /// </summary>
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<LocalFileStorageService> _logger;
        private readonly string _basePath;
        private readonly string _audioFolder;
        private readonly string _baseUrl;

        public LocalFileStorageService(IConfiguration configuration, ILogger<LocalFileStorageService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _basePath = _configuration["FileStorage:Local:BasePath"] ?? "uploads";
            _audioFolder = _configuration["FileStorage:Local:AudioFolder"] ?? "case_audio";
            _baseUrl = _configuration["FileStorage:Local:BaseUrl"] ?? "/uploads";
        }

        public bool IsAvailable => true;

        public async Task<string> UploadAudioAsync(IFormFile audioFile)
        {
            // 確保目錄存在
            var fullDir = Path.Combine(Directory.GetCurrentDirectory(), _basePath, _audioFolder);
            Directory.CreateDirectory(fullDir);

            // 產生唯一檔名
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var guid = Guid.NewGuid().ToString("N")[..8];
            var extension = Path.GetExtension(audioFile.FileName).ToLowerInvariant();
            var fileName = $"{timestamp}_{guid}{extension}";

            var filePath = Path.Combine(fullDir, fileName);

            _logger.LogInformation("儲存音檔到本地: {Path}", filePath);

            using var stream = new FileStream(filePath, FileMode.Create);
            await audioFile.CopyToAsync(stream);

            var url = $"{_baseUrl}/{_audioFolder}/{fileName}";
            _logger.LogInformation("本地音檔 URL: {Url}", url);

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
            var connectionString = _configuration.GetConnectionString("AzureStorage");
            var containerName = _configuration["AzureStorage:ContainerName"];
            var audioFolder = _configuration["AzureStorage:AudioFolder"] ?? "case_audio/";

            if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(containerName))
                throw new InvalidOperationException("Azure Storage 設定未配置");

            _logger.LogInformation("上傳音檔到 Azure Blob Storage...");

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();

            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var guid = Guid.NewGuid().ToString("N")[..8];
            var extension = Path.GetExtension(audioFile.FileName).ToLowerInvariant();
            var fileName = $"{audioFolder}{timestamp}_{guid}{extension}";

            var blobClient = containerClient.GetBlobClient(fileName);

            using var stream = new MemoryStream();
            await audioFile.CopyToAsync(stream);
            stream.Position = 0;
            await blobClient.UploadAsync(stream, overwrite: true);

            var url = blobClient.Uri.ToString();
            _logger.LogInformation("Azure Blob 上傳成功: {Url}", url);

            return url;
        }
    }
}
