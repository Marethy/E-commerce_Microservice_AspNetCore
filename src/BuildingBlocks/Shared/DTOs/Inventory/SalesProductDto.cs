using Shared.Enums.Inventory;

namespace Shared.DTOs.Inventory;

public record SalesProductDto(string ExternalDocumentNo, int Quantity)
{
    public DocumentType DocumentType = DocumentType.Sale;

    public string ItemNo { get; set; }
    public void SetItemNo(string itemNo)
    {
        ItemNo = itemNo;
    }
}