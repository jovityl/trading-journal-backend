using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TradingJournal.Domain.Interfaces;
using TradingJournal.Infrastructure.Persistence;

namespace TradingJournal.Infrastructure.Implementations
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly TradingJournalDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(TradingJournalDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(object id)
            => await _dbSet.FindAsync(id);

        public async Task<T?> GetOneAsync(Expression<Func<T, bool>>? filter = null)
        {
            IQueryable<T> query = _dbSet;
            if (filter != null) query = query.Where(filter);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<T>> GetListAsync(
            Expression<Func<T, bool>>? filter = null,
            int? pageSize = null,
            int? pageNumber = null)
        {
            IQueryable<T> query = _dbSet;
            if (filter != null) query = query.Where(filter);
            if (pageSize.HasValue && pageNumber.HasValue)
                query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
            return await query.ToListAsync();
        }

        public async Task<TResult?> GetOneAsyncUntracked<TResult>(
            Expression<Func<T, bool>>? filter = null,
            Expression<Func<T, TResult>>? selector = null)
        {
            IQueryable<T> query = _dbSet.AsNoTracking();
            if (filter != null) query = query.Where(filter);
            return selector != null
                ? await query.Select(selector).FirstOrDefaultAsync()
                : await query.Cast<TResult>().FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<TResult>> GetListAsyncUntracked<TResult>(
            Expression<Func<T, bool>>? filter = null,
            Expression<Func<T, TResult>>? selector = null,
            int? pageSize = null,
            int? pageNumber = null)
        {
            IQueryable<T> query = _dbSet.AsNoTracking();
            if (filter != null) query = query.Where(filter);
            if (pageSize.HasValue && pageNumber.HasValue)
                query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
            return selector != null
                ? await query.Select(selector).ToListAsync()
                : await query.Cast<TResult>().ToListAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
            => await _context.SaveChangesAsync();
    }
}
