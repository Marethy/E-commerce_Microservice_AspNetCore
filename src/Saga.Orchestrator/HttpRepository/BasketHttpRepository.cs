using Infrastructure.Extensions;
using Saga.Orchestrator.HttpRepository.Interfaces;
using Shared.DTOs.Basket;
using Shared.SeedWork.ApiResult;

namespace Saga.Orchestrator.HttpRepository;

public class BasketHttpRepository : IBasketHttpRepository
{
    private readonly HttpClient _client;

    public BasketHttpRepository(HttpClient client)
    {
        _client = client;
    }

    public async Task<CartDto> GetBasketAsync(string userName)
    {
        var response = await _client.GetAsync($"baskets/{userName}");
        if (!response.IsSuccessStatusCode) return null;

        var apiResult = await response.ReadContentAs<ApiSuccessResult<CartDto>>();
        if (apiResult?.Data == null || !apiResult.Data.Items.Any()) return null;

        return apiResult.Data;
    }

    public async Task<bool> DeleteBasketAsync(string userName)
    {
        var response = await _client.DeleteAsync($"baskets/{userName}");
        return response.IsSuccessStatusCode;
    }
}