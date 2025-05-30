using AutoMapper;
using Contracts.Sagas.OrderManager;
using Saga.Orchestrator.HttpRepository.Interfaces;
using Shared.DTOs.Basket;
using Shared.DTOs.Inventory;
using Shared.DTOs.Order;

namespace Saga.Orchestrator.OrderManager;

public class SagaOrderManager : ISagaOrderManager<BasketCheckoutDto, OrderResponse>
{
    private readonly IMapper _mapper;
    private readonly IOrderHttpRepository _orderHttpRepository;
    private readonly IBasketHttpRepository _basketHttpRepository;
    private readonly IInventoryHttpRepository _inventoryHttpRepository;

    public SagaOrderManager(IMapper mapper,
                            IOrderHttpRepository orderHttpRepository,
                            IBasketHttpRepository basketHttpRepository,
                            IInventoryHttpRepository inventoryHttpRepository)
    {
        _mapper = mapper;
        _orderHttpRepository = orderHttpRepository;
        _basketHttpRepository = basketHttpRepository;
        _inventoryHttpRepository = inventoryHttpRepository;
    }

    public OrderResponse CreateOrder(BasketCheckoutDto input)
    {
        var orderStateMachine = new Stateless.StateMachine<EOrderTransactionState, EOrderAction>(EOrderTransactionState.NotStarted);

        long orderId = -1;
        CartDto cart = null;
        OrderDto addedOrder = null;
        string inventoryDocumentNo = null;

        // 1. GetBasket by UserName
        orderStateMachine.Configure(EOrderTransactionState.NotStarted)
            .PermitDynamic(EOrderAction.GetBasket, () =>
            {
                cart = _basketHttpRepository.GetBasketAsync(input.UserName).Result;
                return cart != null ? EOrderTransactionState.BasketGot : EOrderTransactionState.BasketGetFailed;
            });

        // 2. Create Order (Place Order)
        orderStateMachine.Configure(EOrderTransactionState.BasketGot)
            .PermitDynamic(EOrderAction.CreateOrder, () =>
            {
                var order = _mapper.Map<CreateOrderDto>(input);
                order.TotalPrice = cart.TotalPrice;
                orderId = _orderHttpRepository.CreateOrderAsync(order).Result;
                return orderId > 0 ? EOrderTransactionState.OrderCreated : EOrderTransactionState.OrderCreateFailed;
            })
            .OnEntry(() => orderStateMachine.Fire(EOrderAction.CreateOrder));

        // 3. Get Order Detail by OrderId
        orderStateMachine.Configure(EOrderTransactionState.OrderCreated)
            .PermitDynamic(EOrderAction.GetOrder, () =>
            {
                addedOrder = _orderHttpRepository.GetOrderAsync(orderId).Result;
                return addedOrder != null ? EOrderTransactionState.OrderGot : EOrderTransactionState.OrderGetFailed;
            })
            .OnEntry(() => orderStateMachine.Fire(EOrderAction.GetOrder));

        // 4. Inventory Update
        orderStateMachine.Configure(EOrderTransactionState.OrderGot)
            .PermitDynamic(EOrderAction.UpdateInventory, () =>
            {
                var salesOrder = new SalesOrderDto()
                {
                    OrderNo = addedOrder.DocumentNo,
                    SaleItems = _mapper.Map<List<SaleItemDto>>(cart.Items)
                };
                inventoryDocumentNo = _inventoryHttpRepository.CreateSalesOrderAsync(salesOrder).Result;
                return inventoryDocumentNo != null ? EOrderTransactionState.InventoryUpdated : EOrderTransactionState.InventoryUpdateFailed;
            })
            .OnEntry(() => orderStateMachine.Fire(EOrderAction.UpdateInventory));

        // 5. Delete Basket
        orderStateMachine.Configure(EOrderTransactionState.InventoryUpdated)
            .PermitDynamic(EOrderAction.DeleteBasket, () =>
            {
                var result = _basketHttpRepository.DeleteBasketAsync(input.UserName).Result;
                return result ? EOrderTransactionState.BasketDeleted : EOrderTransactionState.InventoryUpdateFailed;
            })
            .OnEntry(() => orderStateMachine.Fire(EOrderAction.DeleteBasket));

        // Rollback Order
        orderStateMachine.Configure(EOrderTransactionState.InventoryUpdateFailed)
            .PermitDynamic(EOrderAction.DeleteInventory, () =>
            {
                RollbackOrder(input.UserName, inventoryDocumentNo, orderId);
                return EOrderTransactionState.InventoryRollback;
            })
            .OnEntry(() => orderStateMachine.Fire(EOrderAction.DeleteInventory));

        orderStateMachine.Fire(EOrderAction.GetBasket);

        return new OrderResponse(orderStateMachine.State == EOrderTransactionState.BasketDeleted);
    }

    public OrderResponse RollbackOrder(string userName, string documentNo, long orderId)
    {
        var orderStateMachine = new Stateless.StateMachine<EOrderTransactionState, EOrderAction>(EOrderTransactionState.NotStartedRollback);

        // 1. Delete Inventory by DocumentNo
        orderStateMachine.Configure(EOrderTransactionState.NotStartedRollback)
            .PermitDynamic(EOrderAction.DeleteInventory, () =>
            {
                _inventoryHttpRepository.DeleteOrderByDocumentNoAsync(documentNo);
                return EOrderTransactionState.InventoryDeleted;
            });

        // 2. Delete Order by OrderId
        orderStateMachine.Configure(EOrderTransactionState.InventoryDeleted)
            .PermitDynamic(EOrderAction.DeleteOrder, () =>
            {
                var result = _orderHttpRepository.DeleteOrderAsync(orderId).Result;
                return result ? EOrderTransactionState.OrderDeleted : EOrderTransactionState.OrderDeleteFailed;
            })
            .OnEntry(() => orderStateMachine.Fire(EOrderAction.DeleteOrder));

        orderStateMachine.Fire(EOrderAction.DeleteInventory);

        return new OrderResponse(orderStateMachine.State == EOrderTransactionState.OrderDeleted);
    }
}