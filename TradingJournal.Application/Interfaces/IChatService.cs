namespace TradingJournal.Application.Interfaces
{
    public class ChatMessage
    {
        public string Role { get; set; } = string.Empty;      // "user" or "assistant"
        public string Content { get; set; } = string.Empty;
    }

    public interface IChatService
    {
        Task<string> ChatAsync(
            string systemPrompt,
            Stream? contextImageStream,
            string? contextImageContentType,
            List<ChatMessage> messages,
            CancellationToken cancellationToken = default);
    }
}
