namespace TradingJournal.Application.Interfaces
{
    public class ChatMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    public class ChatResult
    {
        public string Reply { get; set; } = string.Empty;
        public AiUsage Usage { get; set; } = new();
    }

    public interface IChatService
    {
        Task<ChatResult> ChatAsync(
            string systemPrompt,
            Stream? contextImageStream,
            string? contextImageContentType,
            List<ChatMessage> messages,
            string? model = null,
            CancellationToken cancellationToken = default);
    }
}
