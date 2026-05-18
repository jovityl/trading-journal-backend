namespace TradingJournal.Application.Interfaces
{
    public class AiScoreResult
    {
        public int Score { get; set; }       // 0-80
        public string Feedback { get; set; } = string.Empty;
    }

    public interface IAiScoringService
    {
        Task<AiScoreResult> ScoreChartAsync(
            Stream chartImageStream,
            string contentType,
            string strategy,
            decimal entryPrice,
            decimal exitPrice,
            string optionType,
            int dte,
            decimal? underlyingEntryPrice = null,
            decimal? underlyingExitPrice = null,
            CancellationToken cancellationToken = default);
    }
}
