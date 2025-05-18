using Basket.API.Entities;
using Basket.API.Repositories.Interfaces;
using Basket.API.Services.Interfaces;
using Contracts.Common.Interfaces;
using Contracts.ScheduledJobs;
using Infrastructure.Common;
using Microsoft.Extensions.Caching.Distributed;
using Shared.DTOs.ScheduledJob;
using ILogger = Serilog.ILogger;

namespace Basket.API.Repositories
{
    public class BasketRepository(IDistributedCache redisCacheService, ISerializeService serializerService, ILogger logger, IBasketEmailService basketEmailService, IScheduledJobsClient scheduledJobsClient) : IBasketRepository
    {
        public async Task<Cart?> GetBasketByUserName(string userName)
        {
            logger.Information($"BEGIN: GetBasketByUserName {userName}");
            var cart = await redisCacheService.GetStringAsync(userName);
            if (!string.IsNullOrEmpty(cart))
            {
                var result = serializerService.Deserialize<Cart>(cart);
                var totalPrice = result.TotalPrice;
                logger.Information("Total price: {totalPrice}", totalPrice); // index totalPrice field into Elastic search
            }
            logger.Information($"END: GetBasketByUserName {userName}");

            return string.IsNullOrEmpty(cart) ? null : serializerService.Deserialize<Cart>(cart);
        }

        public async Task<Cart> UpdateBasket(Cart cart, DistributedCacheEntryOptions options)
        {
            await DeleteReminderCheckoutOrder(cart.Username);

            logger.Information($"BEGIN: UpdateBasket for {cart.Username}");
            var jsonCart = serializerService.Serialize(cart);

            if (options != null)
            {
                await redisCacheService.SetStringAsync(cart.Username, jsonCart, options);
            }
            else
            {
                await redisCacheService.SetStringAsync(cart.Username, jsonCart);
            }

            logger.Information($"END: UpdateBasket for {cart.Username}");

            try
            {
                await TriggerSendEmailReminderCheckoutOrder(cart);
            }
            catch (Exception ex)
            {
                logger.Error($"UpdateBasket: {ex.Message}");
            }

            return await GetBasketByUserName(cart.Username);
        }

        private async Task DeleteReminderCheckoutOrder(string Username)
        {
            var cart = await GetBasketByUserName(Username);
            if (cart == null || string.IsNullOrEmpty(cart.JobId)) return;

            await scheduledJobsClient.DeleteJobAsync(cart.JobId);
        }

        private async Task TriggerSendEmailReminderCheckoutOrder(Cart cart)
        {
            var emailContent = basketEmailService.GenerateReminderCheckoutOrderEmail(cart.Username);

            var model = new ReminderEmailDto(cart.EmailAddress, "Reminder checkout", emailContent, DateTimeOffset.UtcNow.AddMinutes(1));

            var jobId = await scheduledJobsClient.SendReminderEmailAsync(model);
            if (!string.IsNullOrEmpty(jobId))
            {
                cart.JobId = jobId;
                await redisCacheService.SetStringAsync(cart.Username, serializerService.Serialize(cart));
            }
        }

        public async Task<bool> DeleteBasketByUserName(string userName)
        {
            try
            {
                await DeleteReminderCheckoutOrder(userName);

                logger.Information($"BEGIN: DeleteBasketFromUserName {userName}");
                await redisCacheService.RemoveAsync(userName);
                logger.Information($"END: DeleteBasketFromUserName {userName}");

                return true;
            }
            catch (Exception e)
            {
                logger.Error("Error DeleteBasketFromUserName: " + e.Message);
                throw;
            }
        }

      



    }
}