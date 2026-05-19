namespace TradingJournal.Contract.DTOs
{
    public class AdminUserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TradeCount { get; set; }
        public decimal TotalPnl { get; set; }
        public int TotalAiCalls { get; set; }
        public decimal TotalAiCost { get; set; }
    }
}
