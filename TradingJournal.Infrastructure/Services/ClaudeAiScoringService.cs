using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using TradingJournal.Application.Interfaces;

namespace TradingJournal.Infrastructure.Services
{
    public class ClaudeAiScoringService : IAiScoringService
    {
        private readonly HttpClient _httpClient;
        private readonly IPromptService _promptService;
        private readonly string _apiKey;
        private const string ApiUrl = "https://api.anthropic.com/v1/messages";
        private const string Model = "claude-sonnet-4-5";

        public ClaudeAiScoringService(HttpClient httpClient, IConfiguration config, IPromptService promptService)
        {
            _httpClient = httpClient;
            _promptService = promptService;
            _apiKey = config["Anthropic:ApiKey"] ?? throw new InvalidOperationException("Anthropic:ApiKey is not configured.");
        }

        public async Task<AiScoreResult> ScoreChartAsync(
            Stream chartImageStream,
            string contentType,
            string strategy,
            decimal entryPrice,
            decimal exitPrice,
            string optionType,
            int dte,
            decimal? underlyingEntryPrice = null,
            decimal? underlyingExitPrice = null,
            CancellationToken cancellationToken = default)
        {
            using var ms = new MemoryStream();
            await chartImageStream.CopyToAsync(ms, cancellationToken);
            var base64Image = Convert.ToBase64String(ms.ToArray());

            var template = await _promptService.GetAsync(PromptKeys.AiScoring, cancellationToken);
            var prompt = FillPlaceholders(template, strategy, entryPrice, exitPrice, optionType, dte, underlyingEntryPrice, underlyingExitPrice);

            var requestBody = new
            {
                model = Model,
                max_tokens = 1024,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new
                            {
                                type = "image",
                                source = new
                                {
                                    type = "base64",
                                    media_type = contentType,
                                    data = base64Image
                                }
                            },
                            new { type = "text", text = prompt }
                        }
                    }
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
                return new AiScoreResult
                {
                    Score = 0,
                    Feedback = $"AI scoring failed: {response.StatusCode}"
                };
            }

            return ParseResponse(responseString);
        }

        private static string FillPlaceholders(string template, string strategy, decimal entryPrice, decimal exitPrice, string optionType, int dte, decimal? underlyingEntry, decimal? underlyingExit)
        {
            var underlyingInfo = underlyingEntry.HasValue && underlyingExit.HasValue
                ? $"- Underlying entry price: ${underlyingEntry}\n- Underlying exit price: ${underlyingExit}\nUse these to locate the entry and exit points on the chart and assess timing precisely."
                : "- Underlying entry/exit price: not provided (use chart context to estimate where the trade happened)";

            return template
                .Replace("{strategy}", strategy)
                .Replace("{optionType}", optionType)
                .Replace("{entryPrice}", entryPrice.ToString())
                .Replace("{exitPrice}", exitPrice.ToString())
                .Replace("{dte}", dte.ToString())
                .Replace("{underlyingInfo}", underlyingInfo);
        }

        private static AiScoreResult ParseResponse(string responseJson)
        {
            try
            {
                using var doc = JsonDocument.Parse(responseJson);
                var text = doc.RootElement
                    .GetProperty("content")[0]
                    .GetProperty("text")
                    .GetString() ?? "";

                text = text.Trim();
                if (text.StartsWith("```")) text = text.Substring(text.IndexOf('\n') + 1);
                if (text.EndsWith("```")) text = text.Substring(0, text.LastIndexOf("```")).Trim();

                using var inner = JsonDocument.Parse(text);
                return new AiScoreResult
                {
                    Score = inner.RootElement.GetProperty("score").GetInt32(),
                    Feedback = inner.RootElement.GetProperty("feedback").GetString() ?? ""
                };
            }
            catch (Exception ex)
            {
                return new AiScoreResult
                {
                    Score = 0,
                    Feedback = $"Failed to parse AI response: {ex.Message}"
                };
            }
        }
    }
}
