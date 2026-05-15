namespace TradingJournal.Contract.Message.Users
{
    public static class Request
    {
        public record CreateUserRequest(
            string Email,
            string DisplayName);

        public record UpdateUserLimitsRequest(
            int DailyLossLimit,
            int DailyProfitTarget);
    }
}
