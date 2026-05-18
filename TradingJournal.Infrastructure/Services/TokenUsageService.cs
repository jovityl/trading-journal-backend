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
        /// Cost calculation per million tokens. Rates depend on the model.
        /// Updated from Anthropic pricing — keep this in sync if rates change.
        /// </summary>
        private static decimal CalculateCost(AiUsage usage)
        {
            var (inputRate, outputRate, cacheWriteRate, cacheReadRate) = GetRates(usage.Model);

            return usage.InputTokens * inputRate
                 + usage.OutputTokens * outputRate
                 + usage.CacheCreationTokens * cacheWriteRate
                 + usage.CacheReadTokens * cacheReadRate;
        }

        private static (decimal input, decimal output, decimal cacheWrite, decimal cacheRead) GetRates(string model)
        {
            if (model.Contains("haiku", StringComparison.OrdinalIgnoreCase))
            {
                // Claude Haiku: $1 input / $5 output / $1.25 cache write / $0.10 cache read per 1M
                return (1m / 1_000_000m, 5m / 1_000_000m, 1.25m / 1_000_000m, 0.10m / 1_000_000m);
            }
            // Default: Claude Sonnet 4.5
            return (3m / 1_000_000m, 15m / 1_000_000m, 3.75m / 1_000_000m, 0.30m / 1_000_000m);
        }
    }
}
