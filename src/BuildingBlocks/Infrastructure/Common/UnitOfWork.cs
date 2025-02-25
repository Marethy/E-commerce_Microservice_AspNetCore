using Contracts.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Common
{
    public class UnitOfWork<TContext> : IUnitOfWork<TContext> where TContext : DbContext
    {
        private readonly TContext _context;
        public UnitOfWork(TContext context) => _context = context;  
        public Task<int> CommitAsync()
        {
            return _context.SaveChangesAsync();
        }

        public void Dispose() => _context.Dispose();
    }
}
