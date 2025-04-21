using Shared.SeedWork.Paging;

namespace Shared.DTOs.Inventory;

public class GetInventoryPagingQuery : RequestParameters
{
    private string _itemNo = string.Empty;

    public string? SearchTerm { get; set; }

    public void SetItemNo(string itemNo) => _itemNo = itemNo;
    public string GetItemNo() => _itemNo;

}