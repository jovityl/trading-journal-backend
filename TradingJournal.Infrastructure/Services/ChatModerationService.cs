using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using TradingJournal.Application.Interfaces;

namespace TradingJournal.Infrastructure.Services
{
    /// <summary>
    /// Classifies user messages as on-topic / off-topic before the expensive chat call.
    /// Uses Claude Haiku via OpenRouter — small, fast, cheap, reliable for binary classification.
    /// Prompt is editable from the admin panel (key: chat_moderation).
    /// Falls back to "on-topic" if the call fails so users aren't blocked.
    /// </summary>
    public class ChatModerationService : IChatModerationService
    {
        private readonly HttpClient _httpClient;
        private readonly IPromptService _promptService;
        private readonly string _apiKey;
        private const string ApiUrl = "https://openrouter.ai/api/v1/chat/completions";
        private const string Model = "anthropic/claude-haiku-4.5";

        public ChatModerationService(HttpClient httpClient, IConfiguration config, IPromptService promptService)
        {
            _httpClient = httpClient;
            _promptService = promptService;
            _apiKey = config["OpenRouter:ApiKey"] ?? throw new InvalidOperationException("OpenRouter:ApiKey is not configured.");
        }

        public async Task<ModerationResult> IsOnTopicAsync(string userMessage, CancellationToken cancellationToken = default)
        {
            var template = await _promptService.GetAsync(PromptKeys.ChatModeration, cancellationToken);
            var prompt = template.Replace("{userMessage}", userMessage);

            var requestBody = new
            {
                model = Model,
                max_tokens = 5,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");
            request.Headers.Add("HTTP-Referer", "https://trading-journal.local");
            request.Headers.Add("X-Title", "Trading Journal");
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.SendAsync(request, cancellationToken);
                var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    // fail open — don't block user if rate-limited
                    return new ModerationResult { IsOnTopic = true };
                }

                using var doc = JsonDocument.Parse(responseString);
                var text = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? "";

                return new ModerationResult
                {
                    IsOnTopic = text.Trim().ToUpperInvariant().StartsWith("ON"),
                    Usage = OpenRouterChatService.ParseUsage(doc.RootElement, Model)
                };
            }
            catch
            {
                return new ModerationResult { IsOnTopic = true };
            }
        }
    }
}
