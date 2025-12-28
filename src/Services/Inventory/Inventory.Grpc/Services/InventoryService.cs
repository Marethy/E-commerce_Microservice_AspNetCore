
using Contracts.Inventory;
using Grpc.Core;
using Inventory.Grpc.Repositories.Interface;
using ILogger = Serilog.ILogger;

namespace Inventory.Grpc.Services;

public class InventoryService(ILogger logger, IInventoryRepository inventoryRepository) : StockProtoService.StockProtoServiceBase
{

    public override async Task<StockModel> GetStock(GetStockRequest request, ServerCallContext context)
    {
        try
        {
            logger.Information($"BEGIN Get Stock of ItemNo: {request.ItemNo}");

            var stockQuantity = await inventoryRepository.GetStockQuantity(request.ItemNo);
            
            logger.Information($"Retrieved stock quantity: {stockQuantity} for ItemNo: {request.ItemNo}");
            
            var result = new StockModel()
            {
                Quantity = stockQuantity
            };

            logger.Information($"END Get Stock of ItemNo: {request.ItemNo} - Quantity: {result.Quantity}");

            return result;
        }
        catch (Exception ex)
        {
            logger.Error(ex, $"ERROR getting stock for ItemNo: {request.ItemNo}. Exception: {ex.Message}");
            throw new RpcException(new Status(StatusCode.Internal, $"Failed to get stock: {ex.Message}"));
        }
    }
}
