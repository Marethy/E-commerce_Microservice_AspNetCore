using Contracts.Common.Interfaces;
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

    // ===== ADMIN METHODS =====
    public async Task<(IEnumerable<Order> Orders, int TotalCount)> GetAllOrdersAsync(int page, int limit, string? status = null)
    {
        var query = FindAll();
        
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<Shared.Enums.Order.OrderStatus>(status, true, out var orderStatus))
        {
            query = query.Where(x => x.Status == orderStatus);
        }
        
        var totalCount = await query.CountAsync();
        var orders = await query
            .OrderByDescending(x => x.CreatedDate)
            .Skip(page * limit)
            .Take(limit)
            .ToListAsync();
            
        return (orders, totalCount);
    }

    public async Task<Order?> UpdateOrderStatusAsync(long orderId, string status)
    {
        var order = await GetByIdAsync(orderId);
        if (order == null) return null;
        
        if (Enum.TryParse<Shared.Enums.Order.OrderStatus>(status, true, out var orderStatus))
        {
            order.Status = orderStatus;
            await UpdateAsync(order);
            return order;
        }
        
        return null;
    }

    public async Task<bool> CancelOrderAsync(long orderId, string? reason = null)
    {
        var order = await GetByIdAsync(orderId);
        if (order == null) return false;
        
        // Only allow cancel if Pending or Confirmed
        if (order.Status != Shared.Enums.Order.OrderStatus.Pending && 
            order.Status != Shared.Enums.Order.OrderStatus.Confirmed)
            return false;
            
        order.Status = Shared.Enums.Order.OrderStatus.Cancelled;
        await UpdateAsync(order);
        return true;
    }

    public async Task<bool> HasUserPurchasedProductAsync(string userName, string productNo)
    {
        // This would need OrderItem table - for now return false
        // TODO: Implement with OrderItems when available
        return await FindByCondition(x => 
            x.UserName == userName && 
            x.Status == Shared.Enums.Order.OrderStatus.Delivered)
            .AnyAsync();
    }

    public async Task<Ordering.Application.Common.Models.OrderStatistics> GetOrderStatisticsAsync()
    {
        var allOrders = await FindAll().ToListAsync();
        
        var deliveredOrders = allOrders.Where(x => x.Status == Shared.Enums.Order.OrderStatus.Delivered).ToList();
        var totalRevenue = deliveredOrders.Sum(x => x.TotalPrice);
        var avgOrderValue = deliveredOrders.Any() ? totalRevenue / deliveredOrders.Count : 0;
        
        return new Ordering.Application.Common.Models.OrderStatistics
        {
            Total = allOrders.Count,
            Pending = allOrders.Count(x => x.Status == Shared.Enums.Order.OrderStatus.Pending),
            Confirmed = allOrders.Count(x => x.Status == Shared.Enums.Order.OrderStatus.Confirmed),
            Shipped = allOrders.Count(x => x.Status == Shared.Enums.Order.OrderStatus.Shipped),
            Delivered = deliveredOrders.Count,
            Cancelled = allOrders.Count(x => x.Status == Shared.Enums.Order.OrderStatus.Cancelled),
            TotalRevenue = totalRevenue,
            AverageOrderValue = avgOrderValue
        };
    }
}
