namespace TradingJournal.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Auth0Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int DailyLossLimit { get; set; }
        public int DailyProfitTarget { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<Trade> Trades { get; set; } = new List<Trade>();
    }
}
