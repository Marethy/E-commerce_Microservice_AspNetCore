using Contracts.Common.Interfaces;
using Ordering.Application.Common.Models;
using Ordering.Domain.Entities;

namespace Ordering.Application.Common.Interfaces
{
    public interface IOrderRepository : IRepositoryBase<Order, long>
    {
        Task<IEnumerable<Order>> GetOrdersByUserName(string userName);

        Task<long> CreateOrderAsync(Order order);

        Task<Order> UpdateOrderAsync(Order order);

        Task DeleteOrderAsync(Order order);

        void CreateOrder(Order order);

        void DeleteOrder(Order order);

        Task<(IEnumerable<Order> Orders, int TotalCount)> GetAllOrdersAsync(int page, int limit, string? status = null);
        Task<Order?> UpdateOrderStatusAsync(long orderId, string status);
        Task<bool> CancelOrderAsync(long orderId, string? reason = null);
        Task<bool> HasUserPurchasedProductAsync(string userName, string productNo);
        Task<OrderStatistics> GetOrderStatisticsAsync();
        Task<List<DailyRevenueDto>> GetDailyRevenueAsync(int days = 30);
        Task<List<RevenueByStatusDto>> GetRevenueByStatusAsync();
        Task<List<AverageOrderValueDto>> GetAverageOrderValueAsync(int days = 30);
    }
}
