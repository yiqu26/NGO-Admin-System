using Azure.AI.OpenAI;

namespace NGO_WebAPI_Backend.Services
{
    public class AzureOpenAIService
    {
        private readonly OpenAIClientFactory _factory;
        private readonly ILogger<AzureOpenAIService> _logger;

        public AzureOpenAIService(OpenAIClientFactory factory, ILogger<AzureOpenAIService> logger)
        {
            _factory = factory;
            _logger = logger;

            if (!_factory.IsAvailable)
            {
                _logger.LogWarning("AI 服務未配置 (Provider: {Provider})，AI 優化功能將無法使用", _factory.Provider);
            }
            else
            {
                _logger.LogInformation("AI 服務已就緒 (Provider: {Provider})", _factory.Provider);
            }
        }

        /// <summary>
        /// 優化活動描述文案
        /// </summary>
        public async Task<string> OptimizeActivityDescription(string originalDescription)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(originalDescription))
                {
                    throw new ArgumentException("活動描述不能為空", nameof(originalDescription));
                }

                if (!_factory.IsAvailable)
                {
                    throw new InvalidOperationException("AI 服務未正確配置");
                }

                _logger.LogInformation("開始 AI 優化活動描述，原始內容長度: {Length}，Provider: {Provider}",
                    originalDescription.Length, _factory.Provider);

                var modelName = _factory.GetChatModel();

                var systemPrompt = @"您是一位專業的NGO活動文案專家。請將使用者提供的簡單活動描述改寫成生動、具體、吸引人的版本。

要求：
1. 保持原意和核心資訊不變
2. 增加情感色彩和具體細節
3. 突出活動的社會意義和正面影響
4. 使用溫暖、正面、鼓舞人心的語調
5. 讓讀者能感受到參與的價值和意義
6. 控制在150-300字之間
7. 使用繁體中文回應
8. 避免過度誇張，保持真實可信

請直接提供優化後的活動描述，不需要額外說明。";

                var userPrompt = $"請優化以下活動描述：\n\n{originalDescription}";

                var messages = new List<ChatRequestMessage>
                {
                    new ChatRequestSystemMessage(systemPrompt),
                    new ChatRequestUserMessage(userPrompt)
                };

                var chatCompletionOptions = new ChatCompletionsOptions(modelName, messages);
                var chatCompletion = await _factory.Client!.GetChatCompletionsAsync(chatCompletionOptions);

                if (chatCompletion?.Value?.Choices?.Count > 0)
                {
                    var optimizedText = chatCompletion.Value.Choices[0].Message.Content?.Trim() ?? "";

                    _logger.LogInformation("AI 優化完成，優化後內容長度: {Length}", optimizedText.Length);

                    return optimizedText;
                }

                throw new InvalidOperationException("AI 未返回有效回應");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI 優化活動描述時發生錯誤");
                throw new InvalidOperationException($"AI 優化失敗: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 檢查 AI 服務是否可用
        /// </summary>
        public bool IsAvailable()
        {
            return _factory.IsAvailable;
        }
    }
}
