using TradingJournal.Domain.Entities;
using TradingJournal.Domain.IRepository;
using TradingJournal.Infrastructure.Implementations;
using TradingJournal.Infrastructure.Persistence;

namespace TradingJournal.Infrastructure.Repository
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(TradingJournalDbContext context) : base(context)
        {
        }
    }
}
