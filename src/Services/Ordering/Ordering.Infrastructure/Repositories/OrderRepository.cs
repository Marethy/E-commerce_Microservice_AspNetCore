using Contracts.Common.Interfaces;
using Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Common.Interfaces;
using Ordering.Domain.Entities;
using Ordering.Infrastructure.Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ordering.Infrastructure.Repositories
{
    internal class OrderRepository : RepositoryBase<Order, long, OrderContext>, IOrderRepository
    {
        public OrderRepository(OrderContext context, IUnitOfWork<OrderContext> unitOfWork)
            : base(context, unitOfWork) { }

        public async Task CreateOrderAsync(Order order)
        {
            await AddAsync(order);
        }

        public async Task DeleteOrderAsync(long id)
        {
            var order = await GetByIdAsync(id);
            if (order != null)
            {
                await DeleteAsync(order);
            }
        }

        public async Task<Order> GetOrderAsync(long id)
        {
            return await GetByIdAsync(id);
        }

        public async Task<IEnumerable<Order>> GetOrdersByUsernameAsync(string username)
        {
            return await FindByCondition(o => o.UserName == username)
                .ToListAsync();
        }

        public async Task UpdateOrderAsync(Order order)
        {
            await UpdateAsync(order);
        }
    }
}




