
using Inventory.Grpc.Protos;

namespace Basket.API.GrpcServices;

public class StockItemGrpcService(StockProtoService.StockProtoServiceClient stockProtoService)
{

    public async Task<StockModel> GetStock(string itemNo)
    {
        try
        {
            var stockItemRequest = new GetStockRequest { ItemNo = itemNo };
            return await stockProtoService.GetStockAsync(stockItemRequest);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}