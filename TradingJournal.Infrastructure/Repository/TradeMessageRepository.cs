using TradingJournal.Domain.Entities;
using TradingJournal.Domain.IRepository;
using TradingJournal.Infrastructure.Implementations;
using TradingJournal.Infrastructure.Persistence;

namespace TradingJournal.Infrastructure.Repository
{
    public class TradeMessageRepository : GenericRepository<TradeMessage>, ITradeMessageRepository
    {
        public TradeMessageRepository(TradingJournalDbContext context) : base(context) { }
    }
}
