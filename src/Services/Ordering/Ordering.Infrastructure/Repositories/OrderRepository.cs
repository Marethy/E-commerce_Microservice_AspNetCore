using Contracts.Common.Interfaces;
using Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Common.Interfaces;
using Ordering.Domain.Entities;
using Ordering.Infrastructure.Persistence;

namespace Ordering.Infrastructure.Repositories;

public class OrderRepository : RepositoryBase<Order, long, OrderContext>, IOrderRepository
{
    public OrderRepository(OrderContext dbContext, IUnitOfWork<OrderContext> unitOfWork) : base(dbContext, unitOfWork)
    {
    }

    public async Task<IEnumerable<Order>> GetOrdersByUserName(string userName)
    {
        return await FindByCondition(x => x.UserName.Equals(userName)).ToListAsync();
    }

    public async Task<long> CreateOrderAsync(Order order)
    {
        return await CreateAsync(order);
    }

    public async Task<Order> UpdateOrderAsync(Order order)
    {
        await UpdateAsync(order);
        return order;
    }

    public async Task DeleteOrderAsync(Order order)
    {
        await DeleteAsync(order);
    }

    public void CreateOrder(Order order) => CreateAsync(order);

    public void DeleteOrder(Order order) => DeleteAsync(order);
}