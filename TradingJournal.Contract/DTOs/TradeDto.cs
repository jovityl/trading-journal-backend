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
        public decimal UnderlyingEntryPrice { get; set; }
        public decimal UnderlyingExitPrice { get; set; }
        public int Quantity { get; set; }
        public int Dte { get; set; }
        public DateTime TradeDate { get; set; }
        public decimal Pnl { get; set; }
        public string? Notes { get; set; }
        public string? IbkrScreenshotUrl { get; set; }
        public string? ChartScreenshotUrl { get; set; }
        public int AiScore { get; set; }
        public string? AiFeedback { get; set; }
        public List<string> ViolationTags { get; set; } = [];
        public int DisciplineScore { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
