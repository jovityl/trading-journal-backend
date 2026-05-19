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
            var m = model.ToLowerInvariant();
            // Free tiers — zero cost
            if (m.Contains(":free")) return (0m, 0m, 0m, 0m);

            // Claude
            if (m.Contains("haiku")) return (1m / 1_000_000m, 5m / 1_000_000m, 1.25m / 1_000_000m, 0.10m / 1_000_000m);
            if (m.Contains("sonnet")) return (3m / 1_000_000m, 15m / 1_000_000m, 3.75m / 1_000_000m, 0.30m / 1_000_000m);
            if (m.Contains("opus")) return (15m / 1_000_000m, 75m / 1_000_000m, 18.75m / 1_000_000m, 1.5m / 1_000_000m);

            // OpenAI
            if (m.Contains("gpt-4o-mini")) return (0.15m / 1_000_000m, 0.60m / 1_000_000m, 0m, 0m);
            if (m.Contains("gpt-5") || m.Contains("gpt-4o")) return (2.5m / 1_000_000m, 10m / 1_000_000m, 0m, 0m);

            // DeepSeek
            if (m.Contains("deepseek")) return (0.27m / 1_000_000m, 1.10m / 1_000_000m, 0m, 0m);

            // Gemini
            if (m.Contains("gemini-2.5-flash") || m.Contains("gemini-2.0-flash")) return (0.075m / 1_000_000m, 0.30m / 1_000_000m, 0m, 0m);
            if (m.Contains("gemini-2.5-pro")) return (1.25m / 1_000_000m, 5m / 1_000_000m, 0m, 0m);

            // Default to Sonnet rates so we don't underestimate
            return (3m / 1_000_000m, 15m / 1_000_000m, 0m, 0m);
        }
    }
}
