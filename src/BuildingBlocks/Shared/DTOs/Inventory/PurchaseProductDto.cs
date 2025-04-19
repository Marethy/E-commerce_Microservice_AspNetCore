using Shared.Enums.Inventory;

namespace Shared.DTOs.Inventory;

public record PurchaseProductDto(int Quantity,
                                 EDocumentType DocumentType = EDocumentType.Purchase);