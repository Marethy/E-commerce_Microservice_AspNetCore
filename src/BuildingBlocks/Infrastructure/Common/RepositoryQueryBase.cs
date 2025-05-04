using Contracts.Common.Interfaces;
using Contracts.Domains;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Common
{
    public class RepositoryQueryBase<T, K, TContext> : IRepositoryQueryBase<T, K, TContext>
        where T : EntityBase<K>
        where TContext : DbContext
    {
        protected readonly TContext _context;
        protected readonly DbSet<T> _dbSet;

        public RepositoryQueryBase(TContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
        }

        public IQueryable<T> FindAll(bool trackChanges = false) =>
            trackChanges ? _dbSet : _dbSet.AsNoTracking();

        public IQueryable<T> FindAll(bool trackChanges, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = FindAll(trackChanges);
            return includes.Aggregate(query, (current, include) => current.Include(include));
        }

        public IQueryable<T> FindByCondition(Expression<Func<T, bool>> predicate, bool trackChanges = false) =>
            trackChanges ? _dbSet.Where(predicate) : _dbSet.Where(predicate).AsNoTracking();

        public IQueryable<T> FindByCondition(Expression<Func<T, bool>> predicate, bool trackChanges, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = FindByCondition(predicate, trackChanges);
            return includes.Aggregate(query, (current, include) => current.Include(include));
        }

        public async Task<T?> GetByIdAsync(K id) => await _dbSet.FindAsync(id);

        public async Task<T?> GetByIdAsync(K id, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            query = includes.Aggregate(query, (current, include) => current.Include(include));
            return await query.SingleOrDefaultAsync(e => e.Id.Equals(id));
        }
    }
}