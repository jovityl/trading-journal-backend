namespace TradingJournal.Domain.Entities
{
    public class TradeMessage
    {
        public Guid Id { get; set; }
        public Guid TradeId { get; set; }
        public Trade Trade { get; set; } = null!;

        public string Role { get; set; } = string.Empty;     // "user" or "assistant"
        public string Content { get; set; } = string.Empty;
        public string? Model { get; set; }                   // which AI model produced assistant message

        public DateTime CreatedAt { get; set; }
    }
}
