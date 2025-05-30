using Shared.DTOs.Basket;

namespace Saga.Orchestrator.HttpRepository.Interfaces;

public interface IBasketHttpRepository
{
    Task<CartDto> GetBasketAsync(string userName);

    Task<bool> DeleteBasketAsync(string userName);
}