using Contracts.Common.Interfaces;
using Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Ordering.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ordering.Application.Common.Interfaces
{
    public interface IOrderRepository: IRepositoryBase<Order,long>
    {
        Task<IEnumerable<Order>> GetOrdersByUserName(string userName);

        Task<long> CreateOrderAsync(Order order);

        Task<Order> UpdateOrderAsync(Order order);

        Task DeleteOrderAsync(Order order);

        void CreateOrder(Order order);

        void DeleteOrder(Order order);
    }
}
