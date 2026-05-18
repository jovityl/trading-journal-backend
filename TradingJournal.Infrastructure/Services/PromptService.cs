using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using TradingJournal.Application.Interfaces;
using TradingJournal.Domain.Entities;
using TradingJournal.Domain.IRepository;

namespace TradingJournal.Infrastructure.Services
{
    /// <summary>
    /// Caches prompts in memory so we don't hit DB on every AI call.
    /// Invalidate() is called by admin endpoint after editing a prompt.
    /// Singleton — uses IServiceScopeFactory to safely access the scoped repository.
    /// </summary>
    public class PromptService : IPromptService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ConcurrentDictionary<string, string> _cache = new();
        private bool _loaded = false;
        private readonly SemaphoreSlim _loadLock = new(1, 1);

        public PromptService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task<string> GetAsync(string key, CancellationToken cancellationToken = default)
        {
            if (!_loaded) await LoadAsync(cancellationToken);
            return _cache.TryGetValue(key, out var content) ? content : string.Empty;
        }

        public void Invalidate()
        {
            _cache.Clear();
            _loaded = false;
        }

        private async Task LoadAsync(CancellationToken cancellationToken)
        {
            await _loadLock.WaitAsync(cancellationToken);
            try
            {
                if (_loaded) return;
                using var scope = _scopeFactory.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IPromptRepository>();
                var prompts = await repo.GetListAsyncUntracked<Prompt>();
                foreach (var p in prompts) _cache[p.Key] = p.Content;
                _loaded = true;
            }
            finally
            {
                _loadLock.Release();
            }
        }
    }
}
