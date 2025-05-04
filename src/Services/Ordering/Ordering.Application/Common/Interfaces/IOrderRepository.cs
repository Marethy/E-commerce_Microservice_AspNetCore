using Contracts.Common.Interfaces;
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
    }
}