using Shared.Enums.Inventory;

namespace Shared.DTOs.Inventory;

public class SaleItemDto
{
    public string ItemNo { get; set; }
    public int Quantity { get; set; }
    public DocumentType DocumentType => DocumentType.Sale;
}