using Contracts.Domains.Interfaces;
using Inventory.Grpc.Entities;

namespace Inventory.Grpc.Repositories.Interface;

public interface IInventoryRepository : IMongoDbRepositoryBase<InventoryEntry>
{
    Task<int> GetStockQuantity(string itemNo);
}