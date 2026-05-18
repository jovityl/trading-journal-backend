namespace TradingJournal.Application.Interfaces
{
    public static class PromptKeys
    {
        public const string AiScoring = "ai_scoring";
        public const string ChatSystem = "chat_system";
    }

    public interface IPromptService
    {
        /// <summary>Get a prompt's content by key. Returns empty if not found.</summary>
        Task<string> GetAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>Invalidate the cache so the next GetAsync re-reads from DB.</summary>
        void Invalidate();
    }
}
