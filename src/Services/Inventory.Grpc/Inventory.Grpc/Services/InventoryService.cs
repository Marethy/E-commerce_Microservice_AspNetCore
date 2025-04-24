
﻿using Grpc.Core;
using Inventory.Grpc.Protos;
using Inventory.Grpc.Repositories.Interfaces;
using ILogger = Serilog.ILogger;

namespace Inventory.Grpc.Services;

public class InventoryService(ILogger logger, IInventoryRepository inventoryRepository) : StockProtoService.StockProtoServiceBase
{

    public override async Task<StockModel> GetStock(GetStockRequest request, ServerCallContext context)
    {
        logger.Information($"BEGIN Get Stock of ItemNo: {request.ItemNo}");

        var stockQuantity = await inventoryRepository.GetStockQuantity(request.ItemNo);
        var result = new StockModel()
        {
            Quantity = stockQuantity
        };

        logger.Information($"END Get Stock of ItemNo: {request.ItemNo} - Quantity: {result.Quantity}");

        return result;
    }
}
