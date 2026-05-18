using Microsoft.AspNetCore.Http;
using TradingJournal.Contract.Abstraction;
using TradingJournal.Contract.Common;
using TradingJournal.Contract.DTOs;

namespace TradingJournal.Contract.Message.Trades
{
    public static class Commands
    {
        public record CreateTradeCommand(
            string Ticker,
            string OptionType,
            string Strategy,
            decimal EntryPrice,
            decimal ExitPrice,
            int Quantity,
            int Dte,
            DateTime TradeDate,
            string? Notes,
            bool HasStopLoss,
            bool HasProfitTarget,
            bool HasPositionSizing,
            bool HasAppropriateDte,
            IFormFile? IbkrScreenshot,
            IFormFile? ChartScreenshot,
            string Auth0Id,
            decimal? UnderlyingEntryPrice = null,
            decimal? UnderlyingExitPrice = null) : ICommand<BaseResponse<TradeDto>>;

        public record DeleteTradeCommand(Guid Id, string Auth0Id) : ICommand<BaseResponse<bool>>;

        public record SeedTradesCommand(string Auth0Id) : ICommand<BaseResponse<int>>;

        public record DeleteAllTradesCommand(string Auth0Id) : ICommand<BaseResponse<int>>;

        public record ChatMessageDto(string Role, string Content);

        public record ChatTradeCommand(
            Guid TradeId,
            List<ChatMessageDto> Messages,
            string Auth0Id) : ICommand<BaseResponse<string>>;
    }
}
