using Common.Logging;
using Contracts.Sagas.OrderManager;
using Infrastructure.Policies;
using Saga.Orchestrator.HttpRepository;
using Saga.Orchestrator.HttpRepository.Interfaces;
using Saga.Orchestrator.OrderManager;
using Saga.Orchestrator.Service.Interface;
using Saga.Orchestrator.Services;
using Shared.DTOs.Basket;

namespace Saga.Orchestrator.Extensions;

public static class ServiceExtensions
{
    public static void ConfigureServices(this IServiceCollection services)
    {
        services.AddTransient<ICheckoutService, CheckoutService>();
        services.AddTransient<ISagaOrderManager<BasketCheckoutDto, OrderResponse>, SagaOrderManager>();
        services.AddTransient<LoggingDelegatingHandler>();
    }

    public static void ConfigureHttpRepository(this IServiceCollection services)
    {
        services.AddScoped<IBasketHttpRepository, BasketHttpRepository>();
        services.AddScoped<IOrderHttpRepository, OrderHttpRepository>();
        services.AddScoped<IInventoryHttpRepository, InventoryHttpRepository>();
    }

    public static void ConfigureHttpClients(this IServiceCollection services)
    {
        ConfigureBasketHttpClients(services);
        ConfigureOrderHttpClients(services);
        ConfigureInventoryHttpClients(services);
    }

    private static void ConfigureBasketHttpClients(this IServiceCollection services)
    {
        services.AddHttpClient<IBasketHttpRepository, BasketHttpRepository>("BasketAPI", (sp, cl) =>
        {
            cl.BaseAddress = new Uri("http://localhost:5004/api/");
        }).AddHttpMessageHandler<LoggingDelegatingHandler>()
        .UseImmediateHttpRetryPolicy()
        .UseCircuitBreakerPolicy()
        .ConfigureTimeoutPolicy();

        //services.AddScoped(sp => sp.GetService<IHttpClientFactory>().CreateClient("BasketAPI"));
    }

    private static void ConfigureOrderHttpClients(this IServiceCollection services)
    {
        services.AddHttpClient<IOrderHttpRepository, OrderHttpRepository>("OrderAPI", (sp, cl) =>
        {
            cl.BaseAddress = new Uri("http://localhost:5005/api/v1/");
        }).AddHttpMessageHandler<LoggingDelegatingHandler>()
        .UseLinearHttpRetryPolicy();

        //services.AddScoped(sp => sp.GetService<IHttpClientFactory>().CreateClient("OrderAPI"));
    }

    private static void ConfigureInventoryHttpClients(this IServiceCollection services)
    {
        services.AddHttpClient<IInventoryHttpRepository, InventoryHttpRepository>("InventoryAPI", (sp, cl) =>
        {
            cl.BaseAddress = new Uri("http://localhost:5006/api/");
        }).AddHttpMessageHandler<LoggingDelegatingHandler>()
        .UseExponentialHttpRetryPolicy();

        //services.AddScoped(sp => sp.GetService<IHttpClientFactory>().CreateClient("InventoryAPI"));
    }
}