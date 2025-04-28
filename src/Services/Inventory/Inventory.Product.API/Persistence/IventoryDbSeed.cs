using Inventory.Product.API.Entities;
using MongoDB.Driver;
using Shared.Configurations;
using Shared.Enums.Inventory;
using Microsoft.Extensions.Logging;

namespace Inventory.Product.API.Persistence
{
    public class InventoryDbSeed
    {
        private readonly ILogger<InventoryDbSeed> _logger;

        public InventoryDbSeed(ILogger<InventoryDbSeed> logger)
        {
            _logger = logger;
        }

        public async Task SeedDataAsync(IMongoClient mongoClient, MongoDbSettings settings)
        {
            var databaseName = settings.DatabaseName;
            var database = mongoClient.GetDatabase(databaseName);
            var inventoryCollection = database.GetCollection<InventoryEntry>("InventoryEntries");

            _logger.LogInformation("Checking if the 'InventoryEntries' collection is empty.");

            if (await inventoryCollection.EstimatedDocumentCountAsync() == 0)
            {
                _logger.LogInformation("No documents found. Seeding data...");
                await inventoryCollection.InsertManyAsync(GetPreconfiguredInventories());
                _logger.LogInformation("Data seeding completed successfully.");
            }
            else
            {
                _logger.LogInformation("Documents already exist in the collection. Skipping seeding.");
            }
        }

        private List<InventoryEntry> GetPreconfiguredInventories()
        {
            return new List<InventoryEntry>
            {
                new()
                {
                    Quantity = 10,
                    DocumentNo = Guid.NewGuid().ToString(),
                    ItemNo = "Lotus",
                    ExternalDocumentNo = Guid.NewGuid().ToString(),
                    DocumentType = DocumentType.Purchase
                },
                new()
                {
                    ItemNo = "Cadillac",
                    Quantity = 10,
                    DocumentNo = Guid.NewGuid().ToString(),
                    ExternalDocumentNo = Guid.NewGuid().ToString(),
                    DocumentType = DocumentType.Purchase
                },
                new()
                {
                    Quantity = 5,
                    DocumentNo = Guid.NewGuid().ToString(),
                    ItemNo = "Tesla Model Y",
                    ExternalDocumentNo = Guid.NewGuid().ToString(),
                    DocumentType = DocumentType.Purchase
                },
                new()
                {
                    Quantity = 15,
                    DocumentNo = Guid.NewGuid().ToString(),
                    ItemNo = "Mazda CX-5",
                    ExternalDocumentNo = Guid.NewGuid().ToString(),
                    DocumentType = DocumentType.Sale
                },
                new()
                {
                    Quantity = 20,
                    DocumentNo = Guid.NewGuid().ToString(),
                    ItemNo = "Ford Ranger",
                    ExternalDocumentNo = Guid.NewGuid().ToString(),
                    DocumentType = DocumentType.Purchase
                }
            };
        }
    }
}
