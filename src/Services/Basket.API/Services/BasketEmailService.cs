using Basket.API.Services.Interfaces;
using Contracts.Services;
using MongoDB.Driver;
using Shared.Configurations;

namespace Basket.API.Services;

public class BasketEmailService : IBasketEmailService
{
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly UrlSettings _urlSettings;

    public BasketEmailService(IEmailTemplateService emailTemplateService, UrlSettings urlSettings)
    {
        _emailTemplateService = emailTemplateService;
        _urlSettings = urlSettings;
    }

    public string GenerateReminderCheckoutOrderEmail(string userName)
    {
        var emailContent = _emailTemplateService.ReadEmailTemplateContent("reminder-checkout-order");

        var checkoutUrl = $"{_urlSettings.ApiGwUrl}/baskets/{userName}";

        var replacedContent = emailContent.Replace("[userName]", userName)
                                          .Replace("[checkoutUrl]", checkoutUrl);

        return replacedContent;
    }
}