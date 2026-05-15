using TradingJournal.Contract.Abstraction;
using TradingJournal.Contract.Common;
using TradingJournal.Contract.DTOs;

namespace TradingJournal.Contract.Message.Trades
{
    public static class Queries
    {
        public record GetTradesQuery(
            string Auth0Id,
            string? Ticker = null,
            string? OptionType = null,
            string? Strategy = null,
            DateTime? FromDate = null,
            DateTime? ToDate = null,
            int? PageSize = null,
            int? PageNumber = null) : IQuery<BaseResponse<IEnumerable<TradeDto>>>;

        public record GetTradeByIdQuery(Guid Id, string Auth0Id) : IQuery<BaseResponse<TradeDto>>;
    }
}
