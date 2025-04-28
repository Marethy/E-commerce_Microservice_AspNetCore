using Inventory.Grpc.Protos;

namespace Basket.API.GrpcServices;

public class StockItemGrpcService(StockProtoService.StockProtoServiceClient stockProtoService)
{
    private readonly StockProtoService.StockProtoServiceClient _stockProtoService = stockProtoService;

    public async Task<StockModel> GetStock(string itemNo)
    {
        try
        {
            var stockItemRequest = new GetStockRequest { ItemNo = itemNo };
            return await _stockProtoService.GetStockAsync(stockItemRequest);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}