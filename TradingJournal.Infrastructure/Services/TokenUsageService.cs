using TradingJournal.Application.Interfaces;
using TradingJournal.Domain.Entities;
using TradingJournal.Domain.IRepository;

namespace TradingJournal.Infrastructure.Services
{
    public class TokenUsageService : ITokenUsageService
    {
        private readonly ITokenUsageRepository _repository;

        public TokenUsageService(ITokenUsageRepository repository)
        {
            _repository = repository;
        }

        public async Task RecordAsync(Guid userId, string endpoint, AiUsage usage, CancellationToken cancellationToken = default)
        {
            var record = new TokenUsage
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Provider = usage.Provider,
                Model = usage.Model,
                Endpoint = endpoint,
                InputTokens = usage.InputTokens,
                OutputTokens = usage.OutputTokens,
                CacheCreationTokens = usage.CacheCreationTokens,
                CacheReadTokens = usage.CacheReadTokens,
                Cost = CalculateCost(usage),
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(record);
        }

        /// <summary>
        /// Cost calculation per million tokens. Claude Sonnet 4.5 pricing:
        ///   input: $3 / 1M tokens
        ///   output: $15 / 1M tokens
        ///   cache write: $3.75 / 1M tokens
        ///   cache read: $0.30 / 1M tokens (90% discount)
        /// </summary>
        private static decimal CalculateCost(AiUsage usage)
        {
            const decimal inputRate = 3m / 1_000_000m;
            const decimal outputRate = 15m / 1_000_000m;
            const decimal cacheWriteRate = 3.75m / 1_000_000m;
            const decimal cacheReadRate = 0.30m / 1_000_000m;

            return usage.InputTokens * inputRate
                 + usage.OutputTokens * outputRate
                 + usage.CacheCreationTokens * cacheWriteRate
                 + usage.CacheReadTokens * cacheReadRate;
        }
    }
}
