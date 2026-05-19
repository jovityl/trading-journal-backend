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
            int EntryQuality,
            int ExitQuality,
            int RiskManagement,
            int PlanAdherence,
            IFormFile? IbkrScreenshot,
            IFormFile? ChartScreenshot,
            decimal? UnderlyingEntryPrice = null,
            decimal? UnderlyingExitPrice = null);
    }
}
