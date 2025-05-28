
using Infrastructure.Extensions;
using Saga.Orchestrator.HttpRepository.Interfaces;
using Shared.DTOs.Order;
using Shared.SeedWork.ApiResult;

namespace Saga.Orchestrator.HttpRepository;

public class OrderHttpRepository : IOrderHttpRepository
{
    private readonly HttpClient _client;

    public OrderHttpRepository(HttpClient client)
    {
        _client = client;
    }

    public async Task<long> CreateOrderAsync(CreateOrderDto order)
    {
        var response = await _client.PostAsJsonAsync("orders", order);
        if (!response.EnsureSuccessStatusCode().IsSuccessStatusCode) return -1;

        var orderId = await response.ReadContentAs<ApiSuccessResult<long>>();
        return orderId.Data;
    }

    public async Task<OrderDto> GetOrderAsync(long id)
    {
        var order = await _client.GetFromJsonAsync<ApiSuccessResult<OrderDto>>($"orders/{id}");
        return order.Data;
    }

    public async Task<bool> DeleteOrderAsync(long id)
    {
        var response = await _client.DeleteAsync($"orders/{id}");
        return response.IsSuccessStatusCode;
    }

    public Task<bool> DeleteOrderByDocumentNo(string documentNo)
    {
        throw new NotImplementedException();
    }
}
