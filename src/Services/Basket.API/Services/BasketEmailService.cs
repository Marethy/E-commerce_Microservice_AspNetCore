using Basket.API.Services.Interfaces;
using Contracts.Services;
using MongoDB.Driver;
using Shared.Configurations;

namespace Basket.API.Services;

public class BasketEmailService(IEmailTemplateService emailTemplateService, UrlSettings urlSettings) : IBasketEmailService
{
    public string GenerateReminderCheckoutOrderEmail(string userName)
    {
        var emailContent = emailTemplateService.ReadEmailTemplateContent("reminder-checkout-order");

        var checkoutUrl = $"{urlSettings.ApiGwUrl}/baskets/{userName}";

        var replacedContent = emailContent.Replace("[userName]", userName)
                                          .Replace("[checkoutUrl]", checkoutUrl);

        return replacedContent;
    }
}