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
        public decimal? UnderlyingEntryPrice { get; set; }   // stock price at entry
        public decimal? UnderlyingExitPrice { get; set; }    // stock price at exit
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

        // Manual discipline ratings (1-5 each, sum = TickedScore 4-20)
        public int EntryQuality { get; set; }       // 1-5
        public int ExitQuality { get; set; }        // 1-5
        public int RiskManagement { get; set; }     // 1-5
        public int PlanAdherence { get; set; }      // 1-5

        // Scores
        public int TickedScore { get; set; }      // 0-20
        public int DisciplineScore { get; set; }  // 0-100 (AiScore + TickedScore)

        public DateTime CreatedAt { get; set; }
    }
}
