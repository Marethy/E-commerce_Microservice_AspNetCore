using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Contracts.Common.Interfaces
{
    public interface IUnitOfWork<TContext> : IDisposable where TContext : DbContext
    {
        Task<int> SaveChangesAsync();
    }
}
