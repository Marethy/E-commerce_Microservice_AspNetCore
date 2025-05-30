namespace Saga.Orchestrator.OrderManager;

public class OrderResponse
{
    public bool Success { get; }

    public OrderResponse(bool success)
    {
        Success = success;
    }
}