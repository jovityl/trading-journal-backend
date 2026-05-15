namespace TradingJournal.Contract.DTOs
{
    public class DashboardDto
    {
        public decimal TodayPnl { get; set; }
        public decimal MonthlyPnl { get; set; }
        public double WinRate { get; set; }
        public double AverageDisciplineScore { get; set; }
        public bool IsDailyLossLimitHit { get; set; }
        public bool IsDailyProfitTargetHit { get; set; }
        public int DailyLossLimit { get; set; }
        public int DailyProfitTarget { get; set; }
        public List<PnlChartDto> PnlChart { get; set; } = new();
        public List<ScoreChartDto> ScoreChart { get; set; } = new();
        public List<TradeDto> RecentTrades { get; set; } = new();
    }

    public class PnlChartDto
    {
        public string Date { get; set; } = string.Empty;
        public decimal Pnl { get; set; }
    }

    public class ScoreChartDto
    {
        public string Date { get; set; } = string.Empty;
        public double AverageScore { get; set; }
    }
}
