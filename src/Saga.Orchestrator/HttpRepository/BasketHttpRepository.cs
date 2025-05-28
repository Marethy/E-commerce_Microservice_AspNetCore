using Saga.Orchestrator.HttpRepository.Interfaces;
using Shared.DTOs.Basket;

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
        var cart = await _client.GetFromJsonAsync<CartDto>($"baskets/{userName}");
        if (cart == null || !cart.Items.Any()) return null;

        return cart;
    }

    public async Task<bool> DeleteBasketAsync(string userName)
    {
        var response = await _client.DeleteAsync($"baskets/{userName}");
        if (!response.EnsureSuccessStatusCode().IsSuccessStatusCode)
            throw new Exception($"Delete basket for: {userName} not success");

        var result = response.IsSuccessStatusCode;
        return result;
    }
}