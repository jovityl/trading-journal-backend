using TradingJournal.Domain.Entities;
using TradingJournal.Domain.IRepository;
using TradingJournal.Infrastructure.Implementations;
using TradingJournal.Infrastructure.Persistence;

namespace TradingJournal.Infrastructure.Repository
{
    public class TradeRepository : GenericRepository<Trade>, ITradeRepository
    {
        public TradeRepository(TradingJournalDbContext context) : base(context)
        {
        }
    }
}
