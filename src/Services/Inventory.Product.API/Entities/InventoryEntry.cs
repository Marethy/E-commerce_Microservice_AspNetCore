using Contracts.Domains;
using Infrastructure.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Shared.Enums.Inventory;

namespace Inventory.Product.API.Entities;

[BsonCollection("InventoryEntries")]
public class InventoryEntry : MongoEntity
{
    public InventoryEntry()
    {
        Id = ObjectId.GenerateNewId().ToString();
        DocumentNo = Guid.NewGuid().ToString();
        ExternalDocumentNo = Guid.NewGuid().ToString();
    }

    [BsonElement("documentType")]
    public DocumentType DocumentType { get; set; }

    [BsonElement("documentNo")]
    public string DocumentNo { get; set; }

    [BsonElement("itemNo")]
    public string ItemNo { get; set; }

    [BsonElement("quantity")]
    public int Quantity { get; set; }

    [BsonElement("externalDocumentNo")]
    public string ExternalDocumentNo { get; set; }
}