using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using TradingJournal.Application.Interfaces;

namespace TradingJournal.Infrastructure.Services
{
    public class ClaudeChatService : IChatService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string ApiUrl = "https://api.anthropic.com/v1/messages";
        private const string DefaultModel = "claude-sonnet-4-5";

        public ClaudeChatService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["Anthropic:ApiKey"] ?? throw new InvalidOperationException("Anthropic:ApiKey is not configured.");
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

            // Build the messages array. For the FIRST user message, prepend the chart image (cached).
            var formattedMessages = new List<object>();
            string? base64Image = null;
            if (contextImageStream is not null && contextImageContentType is not null)
            {
                using var ms = new MemoryStream();
                await contextImageStream.CopyToAsync(ms, cancellationToken);
                base64Image = Convert.ToBase64String(ms.ToArray());
            }

            bool imageAttached = false;
            foreach (var msg in messages)
            {
                if (!imageAttached && msg.Role == "user" && base64Image is not null)
                {
                    // First user message: attach image with cache control
                    formattedMessages.Add(new
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
                                    media_type = contextImageContentType,
                                    data = base64Image
                                },
                                cache_control = new { type = "ephemeral" }
                            },
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
                max_tokens = 512,
                system = new object[]
                {
                    new
                    {
                        type = "text",
                        text = systemPrompt,
                        cache_control = new { type = "ephemeral" }
                    }
                },
                messages = formattedMessages
            };

            var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
            request.Headers.Add("x-api-key", _apiKey);
            request.Headers.Add("anthropic-version", "2023-06-01");
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                return new ChatResult { Reply = $"Chat failed: {response.StatusCode}" };

            try
            {
                using var doc = JsonDocument.Parse(responseString);
                var text = doc.RootElement
                    .GetProperty("content")[0]
                    .GetProperty("text")
                    .GetString() ?? "";

                var usage = ClaudeAiScoringService.ParseUsage(doc.RootElement);
                usage.Model = modelName;
                return new ChatResult { Reply = text, Usage = usage };
            }
            catch (Exception ex)
            {
                return new ChatResult { Reply = $"Failed to parse response: {ex.Message}" };
            }
        }
    }
}
