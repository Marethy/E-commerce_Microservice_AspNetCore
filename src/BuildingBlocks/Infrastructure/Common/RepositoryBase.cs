using Contracts.Common.Interfaces;
using Contracts.Domains;
using Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

public class RepositoryBase<T, K, TContext> : RepositoryQueryBase<T, K, TContext>, IRepositoryBase<T, K, TContext>
    where T : EntityBase<K>
    where TContext : DbContext
{
    private readonly IUnitOfWork<TContext> _unitOfWork;

    public RepositoryBase(TContext context, IUnitOfWork<TContext> unitOfWork) : base(context)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync() => await _context.Database.BeginTransactionAsync();

    public async Task EndTransactionAsync()
    {
        await SaveChangesAsync();
        await _context.Database.CommitTransactionAsync();
    }

    public async Task RollbackTransactionAsync() => await _context.Database.RollbackTransactionAsync();

    public void Create(T entity)
    {
        _dbSet.Add(entity);
        _context.SaveChanges(); // hoặc SaveChangesAsync nếu muốn đồng bộ cách dùng async
    }

    public async Task<K> CreateAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await SaveChangesAsync();
        return entity.Id;
    }

    public IList<K> CreateList(IEnumerable<T> entities)
    {
        _dbSet.AddRange(entities);
        _context.SaveChanges();
        return entities.Select(e => e.Id).ToList();
    }

    public async Task<IList<K>> CreateListAsync(IEnumerable<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);
        await SaveChangesAsync();
        return entities.Select(e => e.Id).ToList();
    }

    public void Update(T entity)
    {
        _dbSet.Update(entity);
        _context.SaveChanges();
    }

    public Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        return SaveChangesAsync();
    }

    public void UpdateList(IList<T> entities)
    {
        _dbSet.UpdateRange(entities);
        _context.SaveChanges();
    }

    public async Task UpdateListAsync(IList<T> entities)
    {
        _dbSet.UpdateRange(entities);
        await SaveChangesAsync();
    }

    public void Delete(T entity)
    {
        _dbSet.Remove(entity);
        _context.SaveChanges();
    }

    public async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await SaveChangesAsync();
    }

    public void DeleteList(IList<T> entities)
    {
        _dbSet.RemoveRange(entities);
        _context.SaveChanges();
    }

    public async Task DeleteListAsync(IList<T> entities)
    {
        _dbSet.RemoveRange(entities);
        await SaveChangesAsync();
    }

    public async Task<int> SaveChangesAsync() => await _unitOfWork.SaveChangesAsync();
}