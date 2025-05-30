using AutoMapper;
using Saga.Orchestrator.HttpRepository.Interfaces;
using Saga.Orchestrator.Services.Interfaces;
using Shared.DTOs.Basket;
using Shared.DTOs.Inventory;
using Shared.DTOs.Order;
using ILogger = Serilog.ILogger;

namespace Saga.Orchestrator.Services;

public class CheckoutService(ILogger logger,
                       IMapper mapper,
                       IBasketHttpRepository basketHttpRepository,
                       IOrderHttpRepository orderHttpRepository,
                       IInventoryHttpRepository inventoryHttpRepository) : ICheckoutService
{
    public async Task<bool> CheckoutOrderAsync(string userName, BasketCheckoutDto basketCheckout)
    {
        // 1. Get cart from BasketHttpRepository
        logger.Information($"Start: Get Cart {userName}");

        var cart = await basketHttpRepository.GetBasketAsync(userName);
        if (cart == null) return false;
        logger.Information($"End: Get Cart {userName} success");

        // 2. Create Order from OrderHttpRepository
        logger.Information($"Start: Create Order");

        var order = mapper.Map<CreateOrderDto>(basketCheckout);
        order.TotalPrice = cart.TotalPrice;
        var orderId = await orderHttpRepository.CreateOrderAsync(order);
        if (orderId < 0) return false;

        // 3. Get Order by OrderId
        var addedOrder = await orderHttpRepository.GetOrderAsync(orderId);
        logger.Information($"End: Created Order success. OrderId: {orderId}. Document No: {addedOrder.DocumentNo}");

        var inventoryDocumentNos = new List<string>();
        bool result;
        try
        {
            // 4. Sales Items from InventoryHttpRepository
            foreach (var item in cart.Items)
            {
                logger.Information($"Start: Sale Item No: {item.ItemNo} - Quantity: {item.Quantity}");

                var saleOrder = new SalesProductDto(addedOrder.DocumentNo, item.Quantity);
                saleOrder.SetItemNo(item.ItemNo);
                var documentNo = await inventoryHttpRepository.CreateSalesItemAsync(saleOrder);
                inventoryDocumentNos.Add(documentNo);
                logger.Information($"End: Sale Item No: {item.ItemNo} " +   
                                    $"- Quantity: {item.Quantity} - Document No: {documentNo}");
            }

            // 5. Delete Basket
            result = await basketHttpRepository.DeleteBasketAsync(userName);
        }
        catch (Exception e)
        {
            logger.Error(e.Message);

            RollbackCheckoutOrder(userName, orderId, inventoryDocumentNos);

            result = false;
        }

        return result;
    }

    private async void RollbackCheckoutOrder(string userName, long orderId, List<string> inventoryDocumentNos)
    {
        logger.Information($"Start: RollbackCheckoutOrder for username: {userName}, " +
                            $"order id: {orderId}, " +
                            $"inventory document nos: {String.Join(", ", inventoryDocumentNos)}");

        var deletedDocumentNos = new List<string>();
        // 1. Delete Order by OrderId
        logger.Information($"Start: Delete Order Id: {orderId}");
        await orderHttpRepository.DeleteOrderAsync(orderId);
        logger.Information($"End: Delete Order Id: {orderId}");

        foreach (var documentNo in inventoryDocumentNos)
        {
            // 2. Delete Inventory by DocumentNo
            await inventoryHttpRepository.DeleteOrderByDocumentNoAsync(documentNo);
            deletedDocumentNos.Add(documentNo);
        }
        logger.Information($"End: Deleted Inventory Document Nos: {String.Join(", ", inventoryDocumentNos)}");
    }
}