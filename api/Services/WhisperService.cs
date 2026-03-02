using System.Net.Http.Headers;
using System.Text.Json;

namespace NGO_WebAPI_Backend.Services
{
    /// <summary>
    /// 透過 HttpClient 呼叫 OpenAI Whisper API 進行語音轉文字
    /// </summary>
    public class WhisperService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<WhisperService> _logger;

        public WhisperService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<WhisperService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Whisper 服務是否可用
        /// </summary>
        public bool IsAvailable
        {
            get
            {
                var provider = _configuration["AI:Provider"];
                var apiKey = _configuration["AI:OpenAI:ApiKey"];
                return provider?.Equals("OpenAI", StringComparison.OrdinalIgnoreCase) == true
                       && !string.IsNullOrEmpty(apiKey);
            }
        }

        /// <summary>
        /// 使用 OpenAI Whisper API 進行語音轉文字
        /// </summary>
        public async Task<WhisperResult> TranscribeAsync(Stream audioStream, string fileName)
        {
            var apiKey = _configuration["AI:OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
                throw new InvalidOperationException("OpenAI API Key 未設定");

            var model = _configuration["AI:OpenAI:WhisperModel"] ?? "whisper-1";
            var language = _configuration["AI:OpenAI:WhisperLanguage"] ?? "zh";

            _logger.LogInformation("開始呼叫 OpenAI Whisper API，model: {Model}, language: {Language}", model, language);

            var client = _httpClientFactory.CreateClient("WhisperAPI");

            using var content = new MultipartFormDataContent();

            // 音檔
            var audioContent = new StreamContent(audioStream);
            audioContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            content.Add(audioContent, "file", fileName);

            // model
            content.Add(new StringContent(model), "model");

            // language
            content.Add(new StringContent(language), "language");

            // response_format = verbose_json (含 duration)
            content.Add(new StringContent("verbose_json"), "response_format");

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/audio/transcriptions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = content;

            var response = await client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Whisper API 呼叫失敗: {StatusCode} - {Body}", response.StatusCode, responseBody);
                throw new InvalidOperationException($"Whisper API 呼叫失敗: {response.StatusCode} - {responseBody}");
            }

            _logger.LogInformation("Whisper API 回應成功");

            // 解析 verbose_json 格式的回應
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            var text = root.GetProperty("text").GetString() ?? "";
            var duration = root.TryGetProperty("duration", out var durationProp)
                ? durationProp.GetDouble()
                : 0.0;

            _logger.LogInformation("Whisper 辨識完成，文字長度: {Length}, 時長: {Duration}s", text.Length, duration);

            return new WhisperResult
            {
                Text = text,
                Duration = duration,
                Confidence = 0.9 // Whisper 沒有提供 confidence，給予預設值
            };
        }
    }

    public class WhisperResult
    {
        public string Text { get; set; } = string.Empty;
        public double Duration { get; set; }
        public double Confidence { get; set; }
    }
}
