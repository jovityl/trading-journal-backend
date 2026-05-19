namespace TradingJournal.Application.Interfaces
{
    public class AiUsage
    {
        public string Provider { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int InputTokens { get; set; }
        public int OutputTokens { get; set; }
        public int CacheCreationTokens { get; set; }
        public int CacheReadTokens { get; set; }
    }

    public class AiScoreResult
    {
        public int Score { get; set; }
        public string Feedback { get; set; } = string.Empty;
        public AiUsage Usage { get; set; } = new();
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
            decimal underlyingEntryPrice,
            decimal underlyingExitPrice,
            CancellationToken cancellationToken = default);
    }
}
