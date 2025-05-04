using Contracts.Domains.Interfaces;
using Inventory.Grpc.Entities;
using Inventory.Product.API.Repositories.Abstraction;

namespace Inventory.Grpc.Repositories.Interface;

public interface IInventoryRepository : IMongoDbRepositoryBase<InventoryEntry>
{
    Task<int> GetStockQuantity(string itemNo);
}