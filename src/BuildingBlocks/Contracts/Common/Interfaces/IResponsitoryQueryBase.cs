using Contracts.Domains;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Contracts.Common.Interfaces
{
    public interface IRepositoryQueryBase<T, K, TContext>
        where T : EntityBase<K>
        where TContext : DbContext
    {
        IQueryable<T> FindAll(bool trackChanges = false);
        IQueryable<T> FindAll(bool trackChanges = false, params Expression<Func<T, object>>[] includes);

        IQueryable<T> FindByCondition(Expression<Func<T, bool>> predicate, bool trackChanges = false);
        IQueryable<T> FindByCondition(Expression<Func<T, bool>> predicate, bool trackChanges = false, params Expression<Func<T, object>>[] includes);

        Task<T?> GetByIdAsync(K id);
        Task<T?> GetByIdAsync(K id, params Expression<Func<T, object>>[] includes);
    }
}
