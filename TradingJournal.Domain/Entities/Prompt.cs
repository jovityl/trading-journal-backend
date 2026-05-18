namespace TradingJournal.Domain.Entities
{
    /// <summary>
    /// Editable prompt template stored in DB so admins can update without redeploying.
    /// Content can contain placeholders like {strategy}, {entryPrice} that get replaced at runtime.
    /// </summary>
    public class Prompt
    {
        public Guid Id { get; set; }
        public string Key { get; set; } = string.Empty;     // unique identifier, e.g. "ai_scoring", "chat_system"
        public string Content { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
    }
}
