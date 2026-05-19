namespace TradingJournal.Contract.DTOs
{
    public class TradeMessageDto
    {
        public Guid Id { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Model { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
