using Shared.Enums.Inventory;

namespace Shared.DTOs.Inventory;

public record InventoryEntryDto(
    string Id,
    DocumentType DocumentType,
    string DocumentNo,
    string ItemNo,
    int Quantity,
    string ExternalDocumentNo
    );