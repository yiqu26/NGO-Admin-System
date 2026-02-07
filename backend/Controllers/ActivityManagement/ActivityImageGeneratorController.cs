using Microsoft.AspNetCore.Mvc;
using Azure.AI.OpenAI;
using System.Text.Json;
using NGO_WebAPI_Backend.Services;

namespace NGO_WebAPI_Backend.Controllers.ActivityManagement
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActivityImageGeneratorController : ControllerBase
    {
        private readonly OpenAIClientFactory _factory;
        private readonly ILogger<ActivityImageGeneratorController> _logger;

        public ActivityImageGeneratorController(OpenAIClientFactory factory, ILogger<ActivityImageGeneratorController> logger)
        {
            _factory = factory;
            _logger = logger;
        }

        [HttpPost("test-connection")]
        public IActionResult TestConnection()
        {
            try
            {
                _logger.LogInformation("開始測試 AI 圖片生成服務連接 (Provider: {Provider})", _factory.Provider);

                if (!_factory.IsAvailable)
                {
                    _logger.LogError("AI 服務配置缺失");
                    return StatusCode(500, new { success = false, message = "AI 服務配置缺失，請檢查 API Key 設定" });
                }

                _logger.LogInformation("AI 圖片生成服務連接測試成功 (Provider: {Provider})", _factory.Provider);

                return Ok(new
                {
                    success = true,
                    message = $"AI 圖片生成服務連接正常 (Provider: {_factory.Provider})"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI 圖片生成服務連接測試失敗");
                return StatusCode(500, new { success = false, message = "AI 圖片生成服務連接測試失敗" });
            }
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateImage([FromBody] ImageGenerationRequest request)
        {
            try
            {
                _logger.LogInformation("開始生成圖片，描述: {Prompt}, Provider: {Provider}", request.Prompt, _factory.Provider);

                if (string.IsNullOrWhiteSpace(request.Prompt))
                {
                    return BadRequest(new { success = false, message = "請提供圖片描述" });
                }

                if (!_factory.IsAvailable)
                {
                    _logger.LogError("AI 服務配置缺失");
                    return StatusCode(500, new { success = false, message = "AI 服務配置錯誤" });
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
                    return StatusCode(500, new { success = false, message = "圖片生成失敗" });
                }

                var imageData = response.Value.Data[0];

                string imageUrl;

                if (!string.IsNullOrEmpty(imageData.Base64Data))
                {
                    imageUrl = $"data:image/png;base64,{imageData.Base64Data}";
                    _logger.LogInformation("使用 Base64 數據");
                }
                else if (!string.IsNullOrEmpty(imageData.Url?.AbsoluteUri))
                {
                    imageUrl = imageData.Url.AbsoluteUri;
                    _logger.LogInformation("使用 URL: {Url}", imageUrl);
                }
                else
                {
                    _logger.LogWarning("圖片生成失敗，既沒有 Base64 數據也沒有 URL");
                    return StatusCode(500, new { success = false, message = "圖片生成失敗，無法獲取圖片數據" });
                }

                _logger.LogInformation("圖片生成成功");

                return Ok(new
                {
                    success = true,
                    imageData = imageUrl,
                    message = "圖片生成成功"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "圖片生成過程中發生錯誤");
                return StatusCode(500, new { success = false, message = "圖片生成失敗，請稍後再試" });
            }
        }
    }

    public class ImageGenerationRequest
    {
        public string Prompt { get; set; } = string.Empty;
    }
}
