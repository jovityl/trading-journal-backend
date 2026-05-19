namespace TradingJournal.Application.Interfaces
{
    /// <summary>
    /// Picks the appropriate IChatService based on the requested model.
    /// Models starting with "claude-" use ClaudeChatService (Anthropic direct).
    /// Everything else uses OpenRouterChatService.
    /// </summary>
    public interface IChatServiceRouter
    {
        IChatService Resolve(string? model);
    }
}
