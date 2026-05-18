namespace TradingJournal.Contract.DTOs
{
    public class UserUsageDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public int TotalCalls { get; set; }
        public int TotalInputTokens { get; set; }
        public int TotalOutputTokens { get; set; }
        public decimal TotalCost { get; set; }
    }

    public class UsageSummaryDto
    {
        public int TotalCalls { get; set; }
        public int TotalInputTokens { get; set; }
        public int TotalOutputTokens { get; set; }
        public decimal TotalCost { get; set; }
        public List<UserUsageDto> PerUser { get; set; } = new();
        public List<UsageCallDto> RecentCalls { get; set; } = new();
    }

    public class UsageCallDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int InputTokens { get; set; }
        public int OutputTokens { get; set; }
        public int CacheReadTokens { get; set; }
        public decimal Cost { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
