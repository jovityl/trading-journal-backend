using TradingJournal.Contract.Abstraction;
using TradingJournal.Contract.Common;
using TradingJournal.Contract.DTOs;

namespace TradingJournal.Contract.Message.Users
{
    public static class Commands
    {
        public record CreateUserCommand(
            string Auth0Id,
            string Email,
            string DisplayName) : ICommand<BaseResponse<UserDto>>;

        public record UpdateUserLimitsCommand(
            string Auth0Id,
            int DailyLossLimit,
            int DailyProfitTarget) : ICommand<BaseResponse<UserDto>>;
    }
}
