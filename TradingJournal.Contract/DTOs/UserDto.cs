namespace TradingJournal.Contract.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int DailyLossLimit { get; set; }
        public int DailyProfitTarget { get; set; }
        public bool IsAdmin { get; set; }
    }
}
