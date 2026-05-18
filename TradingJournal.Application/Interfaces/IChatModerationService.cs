namespace TradingJournal.Application.Interfaces
{
    public class ModerationResult
    {
        public bool IsOnTopic { get; set; }
        public AiUsage Usage { get; set; } = new();
    }

    /// <summary>
    /// Cheap pre-check to filter off-topic questions before expensive chat call.
    /// Uses a small/cheap model (e.g. Claude Haiku) for classification.
    /// </summary>
    public interface IChatModerationService
    {
        Task<ModerationResult> IsOnTopicAsync(string userMessage, CancellationToken cancellationToken = default);
    }
}
