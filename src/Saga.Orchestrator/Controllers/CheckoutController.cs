using Contracts.Sagas.OrderManager;
using Microsoft.AspNetCore.Mvc;
using Saga.Orchestrator.OrderManager;
using Saga.Orchestrator.Services.Interfaces;
using Shared.DTOs.Basket;

namespace Saga.Orchestrator.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CheckoutController : ControllerBase
{
    //private readonly ICheckoutService _checkoutService;
    private readonly ISagaOrderManager<BasketCheckoutDto, OrderResponse> _orderManager;

    public CheckoutController(ICheckoutService checkoutService,
                              ISagaOrderManager<BasketCheckoutDto, OrderResponse> orderManager)
    {
        //_checkoutService = checkoutService;
        _orderManager = orderManager;
    }

    [HttpPost]
    [Route("{userName}")]
    public async Task<IActionResult> CheckoutOrder(string userName, BasketCheckoutDto model)
    {
        //var result = await _checkoutService.CheckoutOrderAsync(userName, model);

        model.UserName = userName;
        var result = _orderManager.CreateOrder(model);
        return Ok(result);
    }
}