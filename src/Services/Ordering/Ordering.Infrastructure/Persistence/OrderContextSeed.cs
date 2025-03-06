using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ordering.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ordering.Infrastructure.Persistence
{
    public static class OrderContextSeed
    {
        public static async Task<IHost> SeedOrderDataAsync(this IHost host)
        {
            using var scope = host.Services.CreateScope();
            var orderContext = scope.ServiceProvider.GetRequiredService<OrderContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<OrderContext>>();

            try
            {
                // Migrate database
                await orderContext.Database.MigrateAsync().ConfigureAwait(false);

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
                            Email = "john.doe@example.com",
                            ShippingAddress = "123 Main St, Anytown, USA",
                            InvoiceAddress = "123 Main St, Anytown, USA",
                            CreatedDate = DateTimeOffset.UtcNow
                        },
                        new Order
                        {
                            UserName = "user2",
                            TotalPrice = 250.00m,
                            FirstName = "Jane",
                            LastName = "Smith",
                            Email = "jane.smith@example.com",
                            ShippingAddress = "456 Elm St, Othertown, USA",
                            InvoiceAddress = "456 Elm St, Othertown, USA",
                            CreatedDate = DateTimeOffset.UtcNow
                        },
                        new Order
                        {
                            UserName = "user3",
                            TotalPrice = 350.00m,
                            FirstName = "Bob",
                            LastName = "Johnson",
                            Email = "bob.johnson@example.com",
                            ShippingAddress = "789 Oak St, Sometown, USA",
                            InvoiceAddress = "789 Oak St, Sometown, USA",
                            CreatedDate = DateTimeOffset.UtcNow
                        }
                    };

                    await orderContext.Orders.AddRangeAsync(orders);
                    await orderContext.SaveChangesAsync();

                    logger.LogInformation("Seeded database with {OrderCount} initial orders.", orders.Count);
                }
                else
                {
                    logger.LogInformation("Database already contains orders. No seeding required.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
            }

            return host;
        }
    }
}
