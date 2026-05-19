using TradingJournal.Application.Interfaces;

namespace TradingJournal.Infrastructure.Services
{
    public class ChatServiceRouter : IChatServiceRouter
    {
        private readonly ClaudeChatService _claude;
        private readonly OpenRouterChatService _openRouter;

        public ChatServiceRouter(ClaudeChatService claude, OpenRouterChatService openRouter)
        {
            _claude = claude;
            _openRouter = openRouter;
        }

        public IChatService Resolve(string? model)
        {
            // Models starting with "claude-" (no vendor prefix) → Anthropic direct (uses our existing credits)
            // Anything else (e.g. "openai/gpt-5", "anthropic/claude-sonnet-4.5", "deepseek/...") → OpenRouter
            if (!string.IsNullOrEmpty(model) && model.StartsWith("claude-", StringComparison.OrdinalIgnoreCase))
                return _claude;
            return _openRouter;
        }
    }
}
