using Infrastructure.Common;
using Inventory.Grpc.Entities;
using Inventory.Grpc.Repositories.Interfaces;
using Inventory.Product.API.Repositories;
using MongoDB.Driver;
using Shared.Configurations;

namespace Inventory.Grpc.Repositories;

public class InventoryRepository : MongoDbRepository<InventoryEntry>, IInventoryRepository
{
    public InventoryRepository(IMongoClient client, MongoDbSettings settings) : base(client, settings)
    {
    }

    public async Task<int> GetStockQuantity(string itemNo)
    {
        var total = Collection.AsQueryable()
                              .Where(x => x.ItemNo.Equals(itemNo))
                              .Sum(x => x.Quantity);
        return total;
    }
}