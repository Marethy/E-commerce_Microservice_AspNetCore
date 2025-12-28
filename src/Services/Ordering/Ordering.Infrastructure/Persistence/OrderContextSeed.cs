using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ordering.Domain.Entities;

namespace Ordering.Infrastructure.Persistence
{
    public static class OrderContextSeed
    {
        public static async Task<IHost> SeedOrderDataAsync(this IHost host)
        {
            using var scope = host.Services.CreateScope();
            var orderContext = scope.ServiceProvider.GetRequiredService<OrderContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<OrderContext>>();

            const int maxRetries = 5;
            var retryCount = 0;

            while (retryCount < maxRetries)
            {
                try
                {
                    logger.LogInformation("Attempting database migration... (Attempt {Retry}/{MaxRetries})", retryCount + 1, maxRetries);
                    await orderContext.Database.MigrateAsync();
                    logger.LogInformation("Database migration completed successfully");
                    break;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    if (retryCount >= maxRetries)
                    {
                        logger.LogError(ex, "Failed to migrate database after {MaxRetries} attempts", maxRetries);
                        throw;
                    }
                    logger.LogWarning(ex, "Database migration failed. Retrying in 5 seconds... (Attempt {Retry}/{MaxRetries})", retryCount, maxRetries);
                    await Task.Delay(5000);
                }
            }

            try
            {
                // Seed data if the table is empty
                if (!orderContext.Orders.Any())
                {
                    logger.LogInformation("Seeding database with initial orders...");

                    var orders = new List<Order>
                    {
                        new Order
                        {
                            UserName = "user1",
                            TotalPrice = 150.00m,
                            FirstName = "John",
                            LastName = "Doe",
                            EmailAddress = "john.doe@example.com",
                            ShippingAddress = "123 Main St, Anytown, USA",
                            InvoiceAddress = "123 Main St, Anytown, USA",
                            Status = Shared.Enums.Order.OrderStatus.Pending,
                            CreatedDate = DateTimeOffset.UtcNow
                        },
                        new Order
                        {
                            UserName = "user2",
                            TotalPrice = 250.00m,
                            FirstName = "Jane",
                            LastName = "Smith",
                            EmailAddress = "jane.smith@example.com",
                            ShippingAddress = "456 Elm St, Othertown, USA",
                            InvoiceAddress = "456 Elm St, Othertown, USA",
                            Status = Shared.Enums.Order.OrderStatus.Confirmed,
                            CreatedDate = DateTimeOffset.UtcNow
                        },
                        new Order
                        {
                            UserName = "user3",
                            TotalPrice = 350.00m,
                            FirstName = "Bob",
                            LastName = "Johnson",
                            EmailAddress = "bob.johnson@example.com",
                            ShippingAddress = "789 Oak St, Sometown, USA",
                            InvoiceAddress = "789 Oak St, Sometown, USA",
                            Status = Shared.Enums.Order.OrderStatus.Delivered,
                            CreatedDate = DateTimeOffset.UtcNow
                        }
                    };

                    await orderContext.Orders.AddRangeAsync(orders);
                    await orderContext.SaveChangesAsync();

                    logger.LogInformation("Seeded database with {OrderCount} initial orders", orders.Count);
                }
                else
                {
                    logger.LogInformation("Database already contains orders. No seeding required");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database");
            }

            return host;
        }
    }
}