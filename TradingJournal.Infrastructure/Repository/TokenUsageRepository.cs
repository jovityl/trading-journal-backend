using TradingJournal.Domain.Entities;
using TradingJournal.Domain.IRepository;
using TradingJournal.Infrastructure.Implementations;
using TradingJournal.Infrastructure.Persistence;

namespace TradingJournal.Infrastructure.Repository
{
    public class TokenUsageRepository : GenericRepository<TokenUsage>, ITokenUsageRepository
    {
        public TokenUsageRepository(TradingJournalDbContext context) : base(context) { }
    }
}
