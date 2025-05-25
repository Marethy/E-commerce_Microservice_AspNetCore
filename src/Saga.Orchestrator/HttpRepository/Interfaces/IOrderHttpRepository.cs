using Ordering.Application.Common.Models;
using Shared.DTOs.Order;

namespace Saga.Orchestrator.HttpRepository.Interfaces;

public interface IOrderHttpRepository
{
    Task<long> CreateOrderAsync(CreateOrderDto order);

    Task<OrderDto> GetOrderAsync(long id);

    Task<bool> DeleteOrderAsync(long id);

    Task<bool> DeleteOrderByDocumentNo(string documentNo);
}