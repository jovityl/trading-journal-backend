using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using TradingJournal.Application.Interfaces;

namespace TradingJournal.Infrastructure.Services
{
    /// <summary>
    /// Classifies a user message as on-topic (trading-related) or off-topic
    /// using Claude Haiku — ~10x cheaper than Sonnet.
    /// </summary>
    public class ClaudeChatModerationService : IChatModerationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string ApiUrl = "https://api.anthropic.com/v1/messages";
        private const string Model = "claude-haiku-4-5";

        public ClaudeChatModerationService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["Anthropic:ApiKey"] ?? throw new InvalidOperationException("Anthropic:ApiKey is not configured.");
        }

        public async Task<ModerationResult> IsOnTopicAsync(string userMessage, CancellationToken cancellationToken = default)
        {
            var prompt = $@"You are a strict classifier for a trading journal chatbot.

The user's message will follow. Decide if it relates to:
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
            request.Headers.Add("x-api-key", _apiKey);
            request.Headers.Add("anthropic-version", "2023-06-01");
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                // If classifier fails, default to allowing the message (fail open)
                return new ModerationResult { IsOnTopic = true };
            }

            try
            {
                using var doc = JsonDocument.Parse(responseString);
                var text = doc.RootElement
                    .GetProperty("content")[0]
                    .GetProperty("text")
                    .GetString() ?? "";

                var usage = ClaudeAiScoringService.ParseUsage(doc.RootElement);
                usage.Model = Model;

                return new ModerationResult
                {
                    IsOnTopic = text.Trim().ToUpperInvariant().StartsWith("ON"),
                    Usage = usage
                };
            }
            catch
            {
                return new ModerationResult { IsOnTopic = true };
            }
        }
    }
}
