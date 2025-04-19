using Contracts.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Contracts.Common.Interfaces
{
    public interface IRepositoryBase<T, K, TContext> : IRepositoryBase<T, K>
         where T : EntityBase<K>
         where TContext : DbContext
    {
    }
    public interface IRepositoryBase<T, K> : IRepositoryQueryBase<T, K>
       where T : EntityBase<K>
    {
        void Create(T entity);
        Task<K> CreateAsync(T entity);
        
        IList<K> CreateList(IEnumerable<T> entities);
        Task<IList<K>> CreateListAsync(IEnumerable<T> entities);

        void Update(T entity);  
        Task UpdateAsync(T entity);

        void UpdateList(IList<T> entities);
        Task UpdateListAsync(IList<T> entities);

        void Delete(T entity);
        Task DeleteAsync(T entity);

        void DeleteList(IList<T> entities);
        Task DeleteListAsync(IList<T> entities);


        Task<int> SaveChangesAsync();

        Task<IDbContextTransaction> BeginTransactionAsync();
        Task EndTransactionAsync();
        Task RollbackTransactionAsync();
    }

}
