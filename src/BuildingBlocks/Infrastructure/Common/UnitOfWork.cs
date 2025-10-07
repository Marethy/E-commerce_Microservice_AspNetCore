using Contracts.Common.Interfaces;
using Contracts.Domains;
using Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Common
{
    public class UnitOfWork<TContext> : IUnitOfWork<TContext> where TContext : DbContext
    {
        private readonly TContext _context;
        private IDbContextTransaction? _transaction;
        private readonly Dictionary<Type, object> _repositories = new();

        public UnitOfWork(TContext context) => _context = context;

        // Repository factory pattern - creates repositories on demand
        public IRepositoryBase<T, K> GetRepository<T, K>() where T : EntityBase<K>
        {
            var entityType = typeof(T);

            if (!_repositories.TryGetValue(entityType, out var repositoryObj))
            {
                // Create repository using the same pattern as your existing RepositoryBase
                var repoType = typeof(RepositoryBase<,,>).MakeGenericType(entityType, typeof(K), typeof(TContext));
                var repoInstance = Activator.CreateInstance(repoType, _context, this);

                if (repoInstance == null)
                    throw new InvalidOperationException($"Cannot create repository for {entityType.Name}");

                _repositories[entityType] = repoInstance;
                repositoryObj = repoInstance;
            }

            return (IRepositoryBase<T, K>)repositoryObj;
        }

        // SaveChanges without cancellation token
        public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();

        // SaveChanges with cancellation token
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken) 
            => _context.SaveChangesAsync(cancellationToken);

        // Transaction management
        public async Task BeginTransactionAsync()
        {
            if (_transaction is not null)
                return; // Already in transaction

            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken)
        {
            if (_transaction is not null)
                return;

            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction is null)
                throw new InvalidOperationException("No active transaction to commit.");

            try
            {
                await _context.SaveChangesAsync();
                await _transaction.CommitAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken)
        {
            if (_transaction is null)
                throw new InvalidOperationException("No active transaction to commit.");

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                await _transaction.CommitAsync(cancellationToken);
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction is null)
                return;

            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken)
        {
            if (_transaction is null)
                return;

            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        // Check for active transaction
        public bool HasActiveTransaction => _transaction is not null;

        // Dispose pattern
        public void Dispose()
        {
            if (_transaction is not null)
            {
                _transaction.Dispose();
                _transaction = null;
            }

            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}