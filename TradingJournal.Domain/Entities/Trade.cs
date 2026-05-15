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

        public decimal EntryPrice { get; set; }
        public decimal ExitPrice { get; set; }
        public int Quantity { get; set; }
        public int Dte { get; set; }
        public DateTime TradeDate { get; set; }

        public decimal Pnl { get; set; } // auto calculated

        public string? Notes { get; set; }
        public string? IbkrScreenshotUrl { get; set; }
        public string? ChartScreenshotUrl { get; set; }

        // AI scoring
        public int AiScore { get; set; }        // 0-80
        public string? AiFeedback { get; set; }

        // Manual discipline ticks
        public bool HasStopLoss { get; set; }
        public bool HasProfitTarget { get; set; }
        public bool HasPositionSizing { get; set; }
        public bool HasAppropriateDte { get; set; }

        // Scores
        public int TickedScore { get; set; }      // 0-20
        public int DisciplineScore { get; set; }  // 0-100 (AiScore + TickedScore)

        public DateTime CreatedAt { get; set; }
    }
}
