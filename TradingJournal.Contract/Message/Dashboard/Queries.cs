using TradingJournal.Contract.Abstraction;
using TradingJournal.Contract.Common;
using TradingJournal.Contract.DTOs;

namespace TradingJournal.Contract.Message.Dashboard
{
    public static class Queries
    {
        public record GetDashboardQuery(string Auth0Id) : IQuery<BaseResponse<DashboardDto>>;
    }
}
