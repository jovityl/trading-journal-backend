using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using TradingJournal.Application.Interfaces;

namespace TradingJournal.Infrastructure.Services
{
    /// <summary>
    /// Classifies user messages using a FREE OpenRouter model.
    /// Defaults to DeepSeek V3 free — solid for simple classification tasks.
    /// Falls back to "on-topic" if the call fails so users aren't blocked.
    /// </summary>
    public class OpenRouterModerationService : IChatModerationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string ApiUrl = "https://openrouter.ai/api/v1/chat/completions";
        private const string Model = "meta-llama/llama-3.3-70b-instruct:free";

        public OpenRouterModerationService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["OpenRouter:ApiKey"] ?? throw new InvalidOperationException("OpenRouter:ApiKey is not configured.");
        }

        public async Task<ModerationResult> IsOnTopicAsync(string userMessage, CancellationToken cancellationToken = default)
        {
            var prompt = $@"You are a strict classifier for a trading journal chatbot.

Decide if the user's message relates to:
- Trading (any style: options, stocks, futures, crypto)
- Trading psychology, discipline, strategy
- The user's specific trade or their journal
- Markets, charts, indicators

Respond with EXACTLY one word: ON or OFF
ON = on-topic
OFF = anything else (jokes, weather, life advice, coding, etc.)

User message: ""{userMessage}""";

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
                    // fail open — don't block user if free tier is rate-limited
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
