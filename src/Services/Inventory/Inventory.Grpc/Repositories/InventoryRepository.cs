using Infrastructure.Common;
using Inventory.Grpc.Entities;
using Inventory.Grpc.Repositories.Interface;
using Inventory.Product.API.Repositories;
using MongoDB.Driver;
using Shared.Configurations;

namespace Inventory.Grpc.Repositories;

public class InventoryRepository(IMongoClient client, MongoDbSettings settings) : MongoDbRepository<InventoryEntry>(client, settings), IInventoryRepository
{
    public async Task<int> GetStockQuantity(string itemNo)
    {
        var filter = Builders<InventoryEntry>.Filter.Eq(x => x.ItemNo, itemNo);
        var cursor = await Collection.FindAsync(filter);
        var items = await cursor.ToListAsync();
        var total = items.Sum(x => x.Quantity);
        return total;
    }
}