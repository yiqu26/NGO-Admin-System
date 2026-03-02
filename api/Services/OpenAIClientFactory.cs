using Azure;
using Azure.AI.OpenAI;

namespace NGO_WebAPI_Backend.Services
{
    /// <summary>
    /// 統一建立 OpenAIClient，支援 OpenAI Direct API 和 Azure OpenAI 雙 provider
    /// </summary>
    public class OpenAIClientFactory
    {
        private readonly OpenAIClient? _client;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OpenAIClientFactory> _logger;
        private readonly string _provider; // "OpenAI" or "Azure"

        public OpenAIClientFactory(IConfiguration configuration, ILogger<OpenAIClientFactory> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // 優先檢查新的 AI 設定區段
            var aiProvider = _configuration["AI:Provider"];

            if (!string.IsNullOrEmpty(aiProvider))
            {
                _provider = aiProvider;
                _logger.LogInformation("使用 AI 設定區段，Provider: {Provider}", _provider);
            }
            else
            {
                // 向下相容：偵測舊的 AzureOpenAI 設定
                var legacyEndpoint = _configuration["AzureOpenAI:Endpoint"];
                var legacyKey = _configuration["AzureOpenAI:ApiKey"];
                if (!string.IsNullOrEmpty(legacyEndpoint) && !string.IsNullOrEmpty(legacyKey))
                {
                    _provider = "Azure";
                    _logger.LogInformation("偵測到舊版 AzureOpenAI 設定，自動使用 Azure provider");
                }
                else
                {
                    _provider = "None";
                    _logger.LogWarning("未找到任何 AI 設定，AI 功能將無法使用");
                    return;
                }
            }

            try
            {
                if (_provider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
                {
                    var apiKey = _configuration["AI:OpenAI:ApiKey"];
                    if (string.IsNullOrEmpty(apiKey))
                    {
                        _logger.LogWarning("OpenAI API Key 未設定，AI 功能將無法使用");
                        return;
                    }

                    _client = new OpenAIClient(apiKey, new OpenAIClientOptions());
                    _logger.LogInformation("OpenAI Direct API client 建立成功");
                }
                else if (_provider.Equals("Azure", StringComparison.OrdinalIgnoreCase))
                {
                    var endpoint = _configuration["AI:Azure:Endpoint"]
                                   ?? _configuration["AzureOpenAI:Endpoint"];
                    var apiKey = _configuration["AI:Azure:ApiKey"]
                                 ?? _configuration["AzureOpenAI:ApiKey"];

                    if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
                    {
                        _logger.LogWarning("Azure OpenAI 設定缺失，AI 功能將無法使用");
                        return;
                    }

                    _client = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
                    _logger.LogInformation("Azure OpenAI client 建立成功");
                }
                else
                {
                    _logger.LogWarning("未知的 AI Provider: {Provider}，AI 功能將無法使用", _provider);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立 OpenAI client 時發生錯誤");
            }
        }

        /// <summary>
        /// 取得 OpenAIClient 實例
        /// </summary>
        public OpenAIClient? Client => _client;

        /// <summary>
        /// AI 服務是否可用
        /// </summary>
        public bool IsAvailable => _client != null;

        /// <summary>
        /// 目前使用的 Provider
        /// </summary>
        public string Provider => _provider;

        /// <summary>
        /// 是否使用 OpenAI Direct API
        /// </summary>
        public bool IsOpenAI => _provider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// 取得 Chat Model 名稱
        /// </summary>
        public string GetChatModel()
        {
            if (IsOpenAI)
                return _configuration["AI:OpenAI:ChatModel"] ?? "gpt-4o-mini";

            // Azure: deployment name
            return _configuration["AI:Azure:DeploymentName"]
                   ?? _configuration["AzureOpenAI:DeploymentName"]
                   ?? "gpt-4";
        }

        /// <summary>
        /// 取得 Image Model 名稱 / Deployment Name
        /// </summary>
        public string GetImageModel()
        {
            if (IsOpenAI)
                return _configuration["AI:OpenAI:ImageModel"] ?? "dall-e-3";

            return _configuration["AI:Azure:DalleDeploymentName"]
                   ?? _configuration["AzureOpenAI:DalleDeploymentName"]
                   ?? "dall-e-3";
        }
    }
}
