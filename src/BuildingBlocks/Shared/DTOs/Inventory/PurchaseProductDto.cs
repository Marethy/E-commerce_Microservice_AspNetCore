using Shared.Enums.Inventory;

namespace Shared.DTOs.Inventory;

public record PurchaseProductDto(int Quantity,
                                 DocumentType DocumentType = DocumentType.Purchase);