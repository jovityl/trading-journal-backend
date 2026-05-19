namespace TradingJournal.Domain.Entities
{
    public class Trade
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public string Ticker { get; set; } = string.Empty;
        public string OptionType { get; set; } = string.Empty; // Call or Put
        public string Strategy { get; set; } = string.Empty;   // Breakout+Retest or Consolidation Zone

        public decimal EntryPrice { get; set; }              // option premium
        public decimal ExitPrice { get; set; }               // option premium
        public decimal UnderlyingEntryPrice { get; set; }
        public decimal UnderlyingExitPrice { get; set; }
        public int Quantity { get; set; }
        public int Dte { get; set; }
        public DateTime TradeDate { get; set; }

        public decimal Pnl { get; set; } // auto calculated

        public string? Notes { get; set; }
        public string? IbkrScreenshotUrl { get; set; }
        public string? ChartScreenshotUrl { get; set; }

        // AI scoring (0-100)
        public int AiScore { get; set; }
        public string? AiFeedback { get; set; }

        // Discipline tracking
        public List<string> ViolationTags { get; set; } = [];
        public int DisciplineScore { get; set; }  // 100 / 70 / 40 / 10 based on tag count

        public DateTime CreatedAt { get; set; }
    }
}
