namespace TradingJournal.Contract.DTOs
{
    public class TradeDto
    {
        public Guid Id { get; set; }
        public string Ticker { get; set; } = string.Empty;
        public string OptionType { get; set; } = string.Empty;
        public string Strategy { get; set; } = string.Empty;
        public decimal EntryPrice { get; set; }
        public decimal ExitPrice { get; set; }
        public int Quantity { get; set; }
        public int Dte { get; set; }
        public DateTime TradeDate { get; set; }
        public decimal Pnl { get; set; }
        public string? Notes { get; set; }
        public string? IbkrScreenshotUrl { get; set; }
        public string? ChartScreenshotUrl { get; set; }
        public int AiScore { get; set; }
        public string? AiFeedback { get; set; }
        public bool HasStopLoss { get; set; }
        public bool HasProfitTarget { get; set; }
        public bool HasPositionSizing { get; set; }
        public bool HasAppropriateDte { get; set; }
        public int TickedScore { get; set; }
        public int DisciplineScore { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
