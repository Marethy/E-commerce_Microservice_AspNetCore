using Shared.SeedWork;

namespace Shared.DTOs.Inventory;

public class GetInventoryPagingQuery : RequestParameters
{
    private string _itemNo;
    public string? SearchTerm { get; set; }

    public string GetItemNo() => _itemNo;

    public void SetItemNo(string itemNo) => _itemNo = itemNo;
}