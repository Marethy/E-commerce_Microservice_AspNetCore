using Shared.DTOs.Inventory;

namespace Saga.Orchestrator.HttpRepository.Interfaces;

public interface IInventoryHttpRepository
{
    Task<string> CreateSalesItemAsync(SalesProductDto model);

    Task<bool> DeleteOrderByDocumentNoAsync(string documentNo);

    Task<string> CreateSalesOrderAsync(SalesOrderDto model);
}