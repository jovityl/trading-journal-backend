using Microsoft.AspNetCore.Http;

namespace TradingJournal.Contract.Message.Trades
{
    public static class Request
    {
        public record CreateTradeRequest(
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
            decimal? UnderlyingEntryPrice = null,
            decimal? UnderlyingExitPrice = null);
    }
}
