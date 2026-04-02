using Microsoft.AspNetCore.Mvc;
using Azure.AI.OpenAI;
using NGO_WebAPI_Backend.Models.Shared;
using NGO_WebAPI_Backend.Services;

namespace NGO_WebAPI_Backend.Controllers.ActivityManagement
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActivityImageGeneratorController : ControllerBase
    {
        private readonly OpenAIClientFactory _factory;
        private readonly ILogger<ActivityImageGeneratorController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public ActivityImageGeneratorController(
            OpenAIClientFactory factory,
            ILogger<ActivityImageGeneratorController> logger,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _factory = factory;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpPost("test-connection")]
        public ActionResult<ApiResponse<object>> TestConnection()
        {
            try
            {
                _logger.LogInformation("開始測試 AI 圖片生成服務連接 (Provider: {Provider})", _factory.Provider);

                if (!_factory.IsAvailable)
                {
                    _logger.LogError("AI 服務配置缺失");
                    return StatusCode(500, ApiResponse<object>.ErrorResponse("AI 服務配置缺失，請檢查 API Key 設定"));
                }

                _logger.LogInformation("AI 圖片生成服務連接測試成功 (Provider: {Provider})", _factory.Provider);
                return Ok(ApiResponse<object>.SuccessResponse(null!, $"AI 圖片生成服務連接正常 (Provider: {_factory.Provider})"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI 圖片生成服務連接測試失敗");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("AI 圖片生成服務連接測試失敗"));
            }
        }

        [HttpPost("generate")]
        public async Task<ActionResult<ApiResponse<string>>> GenerateImage([FromBody] ImageGenerationRequest request)
        {
            try
            {
                _logger.LogInformation("開始生成圖片，描述: {Prompt}, Provider: {Provider}", request.Prompt, _factory.Provider);

                if (string.IsNullOrWhiteSpace(request.Prompt))
                    return BadRequest(ApiResponse<object>.ErrorResponse("請提供圖片描述"));

                if (!_factory.IsAvailable)
                {
                    _logger.LogError("AI 服務配置缺失");
                    return StatusCode(500, ApiResponse<object>.ErrorResponse("AI 服務配置錯誤"));
                }

                var imageModel = _factory.GetImageModel();

                var imageGenerationOptions = new ImageGenerationOptions
                {
                    Prompt = request.Prompt,
                    Size = ImageSize.Size1024x1024,
                    Quality = ImageGenerationQuality.Standard,
                    Style = ImageGenerationStyle.Natural,
                    DeploymentName = imageModel
                };

                var response = await _factory.Client!.GetImageGenerationsAsync(imageGenerationOptions);

                if (response.Value.Data.Count == 0)
                {
                    _logger.LogWarning("圖片生成失敗，沒有返回數據");
                    return StatusCode(500, ApiResponse<object>.ErrorResponse("圖片生成失敗"));
                }

                var imageData = response.Value.Data[0];
                string imageUrl;

                if (!string.IsNullOrEmpty(imageData.Base64Data))
                {
                    imageUrl = await SaveImageToLocalAsync(Convert.FromBase64String(imageData.Base64Data));
                    _logger.LogInformation("Base64 圖片已儲存到本地: {Url}", imageUrl);
                }
                else if (!string.IsNullOrEmpty(imageData.Url?.AbsoluteUri))
                {
                    _logger.LogInformation("從 DALL-E URL 下載圖片: {Url}", imageData.Url.AbsoluteUri);
                    var httpClient = _httpClientFactory.CreateClient();
                    var imageBytes = await httpClient.GetByteArrayAsync(imageData.Url.AbsoluteUri);
                    imageUrl = await SaveImageToLocalAsync(imageBytes);
                    _logger.LogInformation("DALL-E 圖片已下載並儲存到本地: {Url}", imageUrl);
                }
                else
                {
                    _logger.LogWarning("圖片生成失敗，既沒有 Base64 數據也沒有 URL");
                    return StatusCode(500, ApiResponse<object>.ErrorResponse("圖片生成失敗，無法獲取圖片數據"));
                }

                _logger.LogInformation("圖片生成成功 (Provider: {Provider})", _factory.Provider);
                return Ok(ApiResponse<string>.SuccessResponse(imageUrl, $"圖片生成成功 (Provider: {_factory.Provider})"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "圖片生成過程中發生錯誤");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("圖片生成失敗，請稍後再試"));
            }
        }

        private async Task<string> SaveImageToLocalAsync(byte[] imageBytes)
        {
            var basePath = _configuration["FileStorage:Local:BasePath"] ?? "uploads";
            var folder = "activity_images";

            var fullDir = Path.Combine(Directory.GetCurrentDirectory(), basePath, folder);
            Directory.CreateDirectory(fullDir);

            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var guid = Guid.NewGuid().ToString("N")[..8];
            var fileName = $"{timestamp}_{guid}.png";
            var filePath = Path.Combine(fullDir, fileName);

            await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

            var fullUrl = $"{Request.Scheme}://{Request.Host}/uploads/{folder}/{fileName}";
            return fullUrl;
        }
    }

    public class ImageGenerationRequest
    {
        public string Prompt { get; set; } = string.Empty;
    }
}
