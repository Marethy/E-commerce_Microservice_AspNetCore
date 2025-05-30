
namespace Saga.Orchestrator.OrderManager;

public enum EOrderTransactionState
{
    NotStarted,
    NotStartedRollback,

    BasketGot,
    BasketGetFailed,
    BasketDeleted,

    OrderCreated,
    OrderCreateFailed,
    OrderGot,
    OrderGetFailed,
    OrderDeleted,
    OrderDeleteFailed,

    InventoryUpdated,
    InventoryUpdateFailed,
    InventoryRollback,
    InventoryDeleted
}
