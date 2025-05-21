namespace Basket.API.Services.Interfaces;

public interface IBasketEmailService
{
    string GenerateReminderCheckoutOrderEmail(string userName);
}