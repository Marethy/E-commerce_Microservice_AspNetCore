using Contracts.Domains;
using Microsoft.EntityFrameworkCore;

namespace Contracts.Common.Interfaces
{
    public interface IUnitOfWork<TContext> : IDisposable where TContext : DbContext
    {
        // Repository factory
        IRepositoryBase<T, K> GetRepository<T, K>() where T : EntityBase<K>;
        
        // SaveChanges methods
        Task<int> SaveChangesAsync();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        
        // Transaction management
        Task BeginTransactionAsync();
        Task BeginTransactionAsync(CancellationToken cancellationToken);
        Task CommitTransactionAsync();
        Task CommitTransactionAsync(CancellationToken cancellationToken);
        Task RollbackTransactionAsync();
        Task RollbackTransactionAsync(CancellationToken cancellationToken);
        
        // Transaction state
        bool HasActiveTransaction { get; }
    }
}