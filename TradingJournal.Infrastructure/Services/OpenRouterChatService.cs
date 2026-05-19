using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using TradingJournal.Application.Interfaces;

namespace TradingJournal.Infrastructure.Services
{
    /// <summary>
    /// Chat service that routes through OpenRouter — supports any model OpenRouter offers
    /// (Claude, GPT, DeepSeek, Gemini, Llama, etc.) via a single integration.
    /// </summary>
    public class OpenRouterChatService : IChatService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string ApiUrl = "https://openrouter.ai/api/v1/chat/completions";
        private const string DefaultModel = "anthropic/claude-sonnet-4.5";

        public OpenRouterChatService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["OpenRouter:ApiKey"] ?? throw new InvalidOperationException("OpenRouter:ApiKey is not configured.");
        }

        public async Task<ChatResult> ChatAsync(
            string systemPrompt,
            Stream? contextImageStream,
            string? contextImageContentType,
            List<ChatMessage> messages,
            string? model = null,
            CancellationToken cancellationToken = default)
        {
            var modelName = string.IsNullOrEmpty(model) ? DefaultModel : model;
            if (messages.Count == 0)
                return new ChatResult { Reply = "No message provided." };

            string? base64Image = null;
            if (contextImageStream is not null && contextImageContentType is not null)
            {
                using var ms = new MemoryStream();
                await contextImageStream.CopyToAsync(ms, cancellationToken);
                base64Image = Convert.ToBase64String(ms.ToArray());
            }

            var formattedMessages = new List<object>
            {
                new { role = "system", content = systemPrompt }
            };

            bool imageAttached = false;
            foreach (var msg in messages)
            {
                if (!imageAttached && msg.Role == "user" && base64Image is not null)
                {
                    formattedMessages.Add(new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "image_url", image_url = new { url = $"data:{contextImageContentType};base64,{base64Image}" } },
                            new { type = "text", text = msg.Content }
                        }
                    });
                    imageAttached = true;
                }
                else
                {
                    formattedMessages.Add(new { role = msg.Role, content = msg.Content });
                }
            }

            var requestBody = new
            {
                model = modelName,
                messages = formattedMessages,
                max_tokens = 512
            };

            var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");
            request.Headers.Add("HTTP-Referer", "https://trading-journal.local");
            request.Headers.Add("X-Title", "Trading Journal");
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                return new ChatResult { Reply = $"Chat failed: {response.StatusCode} — {responseString}" };

            try
            {
                using var doc = JsonDocument.Parse(responseString);
                var text = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? "";

                return new ChatResult { Reply = text, Usage = ParseUsage(doc.RootElement, modelName) };
            }
            catch (Exception ex)
            {
                return new ChatResult { Reply = $"Failed to parse response: {ex.Message}" };
            }
        }

        internal static AiUsage ParseUsage(JsonElement root, string model)
        {
            var usage = new AiUsage { Provider = "openrouter", Model = model };
            if (root.TryGetProperty("usage", out var u))
            {
                if (u.TryGetProperty("prompt_tokens", out var i)) usage.InputTokens = i.GetInt32();
                if (u.TryGetProperty("completion_tokens", out var o)) usage.OutputTokens = o.GetInt32();
            }
            return usage;
        }
    }
}
