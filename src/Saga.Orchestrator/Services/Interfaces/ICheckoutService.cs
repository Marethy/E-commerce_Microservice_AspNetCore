using Shared.DTOs.Basket;
namespace Saga.Orchestrator.Services.Interfaces;

public interface ICheckoutService
{
    Task<bool> CheckoutOrderAsync(string userName, BasketCheckoutDto basketCheckout);
}