using Microsoft.AspNetCore.Mvc;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Azure.Storage.Blobs;
using System.Text;
using NGO_WebAPI_Backend.Models.Infrastructure;
using NGO_WebAPI_Backend.Models.Shared;
using NGO_WebAPI_Backend.Services;
using Microsoft.EntityFrameworkCore;

namespace NGO_WebAPI_Backend.Controllers.CaseManagement
{
    [ApiController]
    [Route("api/[controller]")]
    public class CaseSpeechToTextController : ControllerBase
    {
        private readonly ILogger<CaseSpeechToTextController> _logger;
        private readonly IConfiguration _configuration;
        private readonly NgoplatformDbContext _context;
        private readonly WhisperService _whisperService;
        private readonly IFileStorageService _fileStorageService;

        public CaseSpeechToTextController(
            ILogger<CaseSpeechToTextController> logger,
            IConfiguration configuration,
            NgoplatformDbContext context,
            WhisperService whisperService,
            IFileStorageService fileStorageService)
        {
            _logger = logger;
            _configuration = configuration;
            _context = context;
            _whisperService = whisperService;
            _fileStorageService = fileStorageService;
        }

        /// <summary>
        /// 上傳音檔（使用 IFileStorageService 委派儲存）
        /// </summary>
        [HttpPost("upload-audio")]
        public async Task<ActionResult<AudioUploadResponse>> UploadAudio(IFormFile audioFile, [FromForm] int? caseId = null)
        {
            try
            {
                _logger.LogInformation("開始處理音檔上傳請求 - caseId: {CaseId}", caseId);

                if (audioFile == null || audioFile.Length == 0)
                    return BadRequest(ApiResponse<object>.ErrorResponse("未提供音檔"));

                if (audioFile.Length > 25 * 1024 * 1024)
                    return BadRequest(ApiResponse<object>.ErrorResponse("音檔大小不能超過 25MB"));

                var allowedExtensions = new[] { ".wav", ".mp3", ".m4a", ".webm", ".ogg" };
                var fileExtension = Path.GetExtension(audioFile.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                    return BadRequest(ApiResponse<object>.ErrorResponse("不支援的音檔格式，請使用 WAV、MP3、M4A、WEBM 或 OGG 格式"));

                if (!_fileStorageService.IsAvailable)
                {
                    _logger.LogError("檔案儲存服務不可用");
                    return StatusCode(500, ApiResponse<object>.ErrorResponse("檔案儲存服務未配置"));
                }

                string audioUrl;
                try
                {
                    audioUrl = await _fileStorageService.UploadAudioAsync(audioFile);
                    _logger.LogInformation("音檔上傳成功，URL: {Url}", audioUrl);
                }
                catch (Exception uploadEx)
                {
                    _logger.LogError(uploadEx, "音檔上傳失敗");
                    return StatusCode(500, ApiResponse<object>.ErrorResponse("音檔上傳失敗", uploadEx.Message));
                }

                // 如果提供了 caseId，更新個案的音檔 URL
                if (caseId.HasValue)
                {
                    var caseItem = await _context.Cases.FindAsync(caseId.Value);
                    if (caseItem != null)
                    {
                        _logger.LogInformation("更新個案 ID: {CaseId} 的音檔 URL", caseId);
                        caseItem.SpeechToTextAudioUrl = audioUrl;
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        _logger.LogWarning("找不到個案 ID: {CaseId}", caseId);
                    }
                }

                var uploadResult = new AudioUploadResponse
                {
                    AudioUrl = audioUrl,
                    FileName = audioFile.FileName,
                    FileSize = audioFile.Length,
                    UploadTime = DateTime.UtcNow
                };
                return Ok(ApiResponse<AudioUploadResponse>.SuccessResponse(uploadResult, "音檔上傳成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "音檔上傳處理失敗");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("音檔上傳失敗，請稍後再試"));
            }
        }

        /// <summary>
        /// 語音轉文字（自動選擇 Whisper 或 Azure Speech）
        /// </summary>
        [HttpPost("transcribe")]
        public async Task<ActionResult<SpeechToTextResponse>> TranscribeAudio(IFormFile audioFile)
        {
            try
            {
                _logger.LogInformation("開始處理語音轉文字請求");

                if (audioFile == null || audioFile.Length == 0)
                    return BadRequest(ApiResponse<object>.ErrorResponse("未提供音檔"));

                if (audioFile.Length > 25 * 1024 * 1024)
                    return BadRequest(ApiResponse<object>.ErrorResponse("音檔大小不能超過 25MB"));

                var allowedExtensions = new[] { ".wav", ".mp3", ".m4a", ".webm", ".ogg" };
                var fileExtension = Path.GetExtension(audioFile.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                    return BadRequest(ApiResponse<object>.ErrorResponse("不支援的音檔格式，請使用 WAV、MP3、M4A、WEBM 或 OGG 格式"));

                // 優先使用 OpenAI Whisper
                if (_whisperService.IsAvailable)
                {
                    _logger.LogInformation("使用 OpenAI Whisper API 進行語音轉文字");

                    using var stream = audioFile.OpenReadStream();
                    var whisperResult = await _whisperService.TranscribeAsync(stream, audioFile.FileName);

                    var whisperDto = new SpeechToTextResponse
                    {
                        Text = whisperResult.Text,
                        Confidence = whisperResult.Confidence,
                        Duration = whisperResult.Duration
                    };
                    return Ok(ApiResponse<SpeechToTextResponse>.SuccessResponse(whisperDto, "語音轉文字成功"));
                }

                // Fallback: Azure Speech Service
                var speechKey = _configuration["AI:AzureSpeech:Key"]
                                ?? _configuration["AzureSpeech:Key"];
                var speechRegion = _configuration["AI:AzureSpeech:Region"]
                                   ?? _configuration["AzureSpeech:Region"];

                if (string.IsNullOrEmpty(speechKey) || string.IsNullOrEmpty(speechRegion))
                {
                    _logger.LogError("語音轉文字服務未配置（Whisper 和 Azure Speech 均不可用）");
                    return StatusCode(500, ApiResponse<object>.ErrorResponse("語音服務未配置，請設定 OpenAI API Key 或 Azure Speech Key"));
                }

                _logger.LogInformation("使用 Azure Speech Service 進行語音轉文字");

                using var memoryStream = new MemoryStream();
                await audioFile.CopyToAsync(memoryStream);
                var audioBytes = memoryStream.ToArray();

                var azureResult = await TranscribeAudioWithAzureSpeechAsync(audioBytes, speechKey, speechRegion);
                return Ok(ApiResponse<SpeechToTextResponse>.SuccessResponse(azureResult, "語音轉文字成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "語音轉文字處理失敗");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("語音轉文字失敗，請稍後再試"));
            }
        }

        /// <summary>
        /// 測試 Azure Blob Storage 連線
        /// </summary>
        [HttpGet("test-blob-storage")]
        public async Task<IActionResult> TestBlobStorageConnection()
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("AzureStorage");
                var containerName = _configuration["AzureStorage:ContainerName"];
                var audioFolder = _configuration["AzureStorage:AudioFolder"] ?? "case_audio/";

                if (string.IsNullOrEmpty(connectionString))
                    return BadRequest(ApiResponse<object>.ErrorResponse("Azure Storage 連接字串未配置"));

                if (string.IsNullOrEmpty(containerName))
                    return BadRequest(ApiResponse<object>.ErrorResponse("Azure Storage 容器名稱未配置"));

                _logger.LogInformation("測試 Azure Blob Storage 連線... Container: {Container}, AudioFolder: {Folder}",
                    containerName, audioFolder);

                var blobServiceClient = new BlobServiceClient(connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                var containerExists = await containerClient.ExistsAsync();
                if (!containerExists.Value)
                {
                    await containerClient.CreateIfNotExistsAsync();
                }

                var blobs = new List<string>();
                await foreach (var blobItem in containerClient.GetBlobsAsync())
                {
                    blobs.Add(blobItem.Name);
                }

                var blobInfo = new
                {
                    containerName,
                    audioFolder,
                    containerExists = containerExists.Value,
                    blobCount = blobs.Count,
                    blobs = blobs.Take(10).ToList()
                };
                return Ok(ApiResponse<object>.SuccessResponse(blobInfo, "Azure Blob Storage 連線測試成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Azure Blob Storage 連線測試失敗");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Azure Blob Storage 連線測試失敗", ex.Message));
            }
        }

        /// <summary>
        /// 測試語音服務連線
        /// </summary>
        [HttpGet("test-connection")]
        public async Task<IActionResult> TestSpeechConnection()
        {
            try
            {
                if (_whisperService.IsAvailable)
                {
                    var whisperInfo = new { provider = "OpenAI Whisper" };
                    return Ok(ApiResponse<object>.SuccessResponse(whisperInfo, "語音轉文字服務正常 (OpenAI Whisper)"));
                }

                var speechKey = _configuration["AI:AzureSpeech:Key"]
                                ?? _configuration["AzureSpeech:Key"];
                var speechRegion = _configuration["AI:AzureSpeech:Region"]
                                   ?? _configuration["AzureSpeech:Region"];

                if (string.IsNullOrEmpty(speechKey) || string.IsNullOrEmpty(speechRegion))
                    return BadRequest(ApiResponse<object>.ErrorResponse("語音服務未配置（Whisper 和 Azure Speech 均不可用）"));

                var config = SpeechConfig.FromSubscription(speechKey, speechRegion);
                config.SpeechRecognitionLanguage = "zh-TW";

                await Task.Delay(100);

                var azureInfo = new { provider = "Azure Speech", region = speechRegion };
                return Ok(ApiResponse<object>.SuccessResponse(azureInfo, "Azure Speech Service 連線設定正常"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "語音服務連線測試失敗");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("連線測試失敗", ex.Message));
            }
        }

        /// <summary>
        /// 從 URL 進行語音轉文字
        /// </summary>
        [HttpPost("transcribe-from-url")]
        public async Task<ActionResult<SpeechToTextResponse>> TranscribeFromUrl([FromBody] TranscribeFromUrlRequest request)
        {
            try
            {
                _logger.LogInformation("開始從 URL 進行語音轉文字: {Url}", request.AudioUrl);

                if (string.IsNullOrEmpty(request.AudioUrl))
                    return BadRequest(ApiResponse<object>.ErrorResponse("未提供音檔 URL"));

                // 優先使用 Whisper
                if (_whisperService.IsAvailable)
                {
                    _logger.LogInformation("使用 OpenAI Whisper API 從 URL 進行語音轉文字");

                    Stream audioStream;
                    string fileName;

                    // 判斷是本地檔案路徑還是遠端 URL
                    if (request.AudioUrl.StartsWith("/uploads/") || request.AudioUrl.StartsWith("uploads/"))
                    {
                        // 本地檔案
                        var localPath = Path.Combine(Directory.GetCurrentDirectory(),
                            request.AudioUrl.TrimStart('/'));
                        if (!System.IO.File.Exists(localPath))
                            return BadRequest(ApiResponse<object>.ErrorResponse("找不到本地音檔"));
                        audioStream = System.IO.File.OpenRead(localPath);
                        fileName = Path.GetFileName(localPath);
                    }
                    else
                    {
                        // 遠端 URL
                        using var httpClient = new HttpClient();
                        var audioBytes = await httpClient.GetByteArrayAsync(request.AudioUrl);
                        audioStream = new MemoryStream(audioBytes);
                        fileName = Path.GetFileName(new Uri(request.AudioUrl).LocalPath);
                        if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
                            fileName = "audio.wav";
                    }

                    using (audioStream)
                    {
                        var whisperResult = await _whisperService.TranscribeAsync(audioStream, fileName);
                        var whisperDto = new SpeechToTextResponse
                        {
                            Text = whisperResult.Text,
                            Confidence = whisperResult.Confidence,
                            Duration = whisperResult.Duration
                        };
                        return Ok(ApiResponse<SpeechToTextResponse>.SuccessResponse(whisperDto, "語音轉文字成功"));
                    }
                }

                var speechKey = _configuration["AI:AzureSpeech:Key"]
                                ?? _configuration["AzureSpeech:Key"];
                var speechRegion = _configuration["AI:AzureSpeech:Region"]
                                   ?? _configuration["AzureSpeech:Region"];

                if (string.IsNullOrEmpty(speechKey) || string.IsNullOrEmpty(speechRegion))
                    return StatusCode(500, ApiResponse<object>.ErrorResponse("語音服務未配置"));

                _logger.LogInformation("使用 Azure Speech Service 從 URL 進行語音轉文字");

                using var client = new HttpClient();
                var bytes = await client.GetByteArrayAsync(request.AudioUrl);

                var azureResult = await TranscribeAudioWithAzureSpeechAsync(bytes, speechKey, speechRegion);
                return Ok(ApiResponse<SpeechToTextResponse>.SuccessResponse(azureResult, "語音轉文字成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "從 URL 語音轉文字處理失敗");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("語音轉文字失敗，請稍後再試"));
            }
        }

        /// <summary>
        /// 使用 Azure Speech Service 進行語音識別 (原有邏輯)
        /// </summary>
        private async Task<SpeechToTextResponse> TranscribeAudioWithAzureSpeechAsync(byte[] audioBytes, string speechKey, string speechRegion)
        {
            try
            {
                _logger.LogInformation("開始配置 Azure Speech Service...");

                var config = SpeechConfig.FromSubscription(speechKey, speechRegion);

                config.SetProperty(PropertyId.SpeechServiceConnection_EndSilenceTimeoutMs, "3000");
                config.SpeechRecognitionLanguage = "zh-TW";
                config.OutputFormat = OutputFormat.Detailed;

                var autoDetectSourceLanguageConfig = AutoDetectSourceLanguageConfig.FromLanguages(
                    new string[] { "zh-TW", "zh-CN", "en-US" });

                config.SetProperty(PropertyId.SpeechServiceConnection_EndSilenceTimeoutMs, "3000");
                config.SetProperty(PropertyId.SpeechServiceConnection_InitialSilenceTimeoutMs, "10000");

                var audioStream = AudioInputStream.CreatePushStream();

                using var audioConfig = AudioConfig.FromStreamInput(audioStream);
                using var recognizer = new SpeechRecognizer(config, audioConfig);

                var startTime = DateTime.UtcNow;

                // 寫入臨時檔案進行識別
                var tempFileName = Path.GetTempFileName() + ".wav";
                await System.IO.File.WriteAllBytesAsync(tempFileName, audioBytes);

                SpeechRecognitionResult result;
                try
                {
                    using var audioConfigFromFile = AudioConfig.FromWavFileInput(tempFileName);
                    using var recognizerFromFile = new SpeechRecognizer(config, audioConfigFromFile);

                    result = await recognizerFromFile.RecognizeOnceAsync();
                }
                catch (Exception fileEx)
                {
                    _logger.LogWarning("檔案識別失敗，回到流識別: {Message}", fileEx.Message);

                    if (audioStream is PushAudioInputStream pushStream)
                    {
                        pushStream.Write(audioBytes);
                        pushStream.Close();
                    }

                    result = await recognizer.RecognizeOnceAsync();
                }
                finally
                {
                    if (System.IO.File.Exists(tempFileName))
                    {
                        System.IO.File.Delete(tempFileName);
                    }
                }

                var duration = (DateTime.UtcNow - startTime).TotalSeconds;

                if (result.Reason == ResultReason.RecognizedSpeech)
                {
                    var confidence = string.IsNullOrEmpty(result.Text) ? 0.0 : 0.8;
                    var resultText = string.IsNullOrEmpty(result.Text) ? "無法識別語音內容" : result.Text;

                    return new SpeechToTextResponse
                    {
                        Text = resultText,
                        Confidence = confidence,
                        Duration = duration
                    };
                }
                else if (result.Reason == ResultReason.NoMatch)
                {
                    return new SpeechToTextResponse
                    {
                        Text = "無法識別語音內容 - 沒有找到匹配的語音",
                        Confidence = 0.0,
                        Duration = duration
                    };
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = CancellationDetails.FromResult(result);
                    _logger.LogError("語音識別被取消: {Reason}, 詳細: {Details}", cancellation.Reason, cancellation.ErrorDetails);
                    return new SpeechToTextResponse
                    {
                        Text = $"語音識別失敗: {cancellation.ErrorDetails}",
                        Confidence = 0.0,
                        Duration = duration
                    };
                }

                return new SpeechToTextResponse
                {
                    Text = "未知的語音識別結果",
                    Confidence = 0.0,
                    Duration = duration
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "語音識別過程中發生錯誤");
                throw;
            }
        }
    }

    /// <summary>
    /// 語音轉文字回應模型
    /// </summary>
    public class SpeechToTextResponse
    {
        public string Text { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public double Duration { get; set; }
        public string AudioUrl { get; set; } = string.Empty;
    }

    /// <summary>
    /// 音檔上傳回應模型
    /// </summary>
    public class AudioUploadResponse
    {
        public string AudioUrl { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadTime { get; set; }
    }

    /// <summary>
    /// 從 URL 轉文字請求模型
    /// </summary>
    public class TranscribeFromUrlRequest
    {
        public string AudioUrl { get; set; } = string.Empty;
    }
}
