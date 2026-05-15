using TradingJournal.Contract.Abstraction;
using TradingJournal.Contract.Common;
using TradingJournal.Contract.DTOs;

namespace TradingJournal.Contract.Message.Users
{
    public static class Queries
    {
        public record GetMeQuery(string Auth0Id) : IQuery<BaseResponse<UserDto>>;
    }
}
