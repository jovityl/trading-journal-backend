using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TradingJournal.Application.Interfaces;

namespace TradingJournal.Infrastructure.Services
{
    public class ClaudeAiScoringService : IAiScoringService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string ApiUrl = "https://api.anthropic.com/v1/messages";
        private const string Model = "claude-sonnet-4-5";

        public ClaudeAiScoringService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
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
            CancellationToken cancellationToken = default)
        {
            // Convert image stream to base64
            using var ms = new MemoryStream();
            await chartImageStream.CopyToAsync(ms, cancellationToken);
            var base64Image = Convert.ToBase64String(ms.ToArray());

            var prompt = BuildPrompt(strategy, entryPrice, exitPrice, optionType, dte);

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

        private static string BuildPrompt(string strategy, decimal entryPrice, decimal exitPrice, string optionType, int dte) => $@"
You are an experienced options trading coach analyzing a chart screenshot.

Trade details:
- Strategy claimed: {strategy}
- Option type: {optionType}
- Entry price: ${entryPrice}
- Exit price: ${exitPrice}
- DTE (days to expiration): {dte}

Evaluate the trade based on what you see in the chart:
1. Was the claimed strategy actually present at entry? (Real breakout + retest or proper consolidation zone?)
2. Was the entry timing reasonable based on the chart structure?
3. Was the exit well-timed, or did they leave money on the table / exit too late?
4. Were there obvious red flags (e.g. entered against the trend, ignored support/resistance)?

Respond ONLY in this exact JSON format (no extra text, no markdown):
{{
  ""score"": <number from 0 to 80>,
  ""feedback"": ""<2-3 sentence explanation of the score, mentioning specific things you see in the chart>""
}}";

        private static AiScoreResult ParseResponse(string responseJson)
        {
            try
            {
                using var doc = JsonDocument.Parse(responseJson);
                var text = doc.RootElement
                    .GetProperty("content")[0]
                    .GetProperty("text")
                    .GetString() ?? "";

                // The text should be JSON, but Claude sometimes adds markdown fencing
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
