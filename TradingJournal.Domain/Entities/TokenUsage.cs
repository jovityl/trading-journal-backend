namespace TradingJournal.Domain.Entities
{
    public class TokenUsage
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public string Provider { get; set; } = string.Empty;   // "anthropic", "openai"
        public string Model { get; set; } = string.Empty;      // "claude-sonnet-4-5"
        public string Endpoint { get; set; } = string.Empty;   // "scoring", "chat"

        public int InputTokens { get; set; }
        public int OutputTokens { get; set; }
        public int CacheCreationTokens { get; set; }
        public int CacheReadTokens { get; set; }
        public decimal Cost { get; set; }   // USD

        public DateTime CreatedAt { get; set; }
    }
}
