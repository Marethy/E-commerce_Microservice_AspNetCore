using Shared.DTOs.Basket;
namespace Saga.Orchestrator.Service.Interfaces;

public interface ICheckoutService
{
    Task<bool> CheckoutOrderAsync(string userName, BasketCheckoutDto basketCheckout);
}