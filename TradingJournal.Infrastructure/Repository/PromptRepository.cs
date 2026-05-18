using TradingJournal.Domain.Entities;
using TradingJournal.Domain.IRepository;
using TradingJournal.Infrastructure.Implementations;
using TradingJournal.Infrastructure.Persistence;

namespace TradingJournal.Infrastructure.Repository
{
    public class PromptRepository : GenericRepository<Prompt>, IPromptRepository
    {
        public PromptRepository(TradingJournalDbContext context) : base(context) { }
    }
}
