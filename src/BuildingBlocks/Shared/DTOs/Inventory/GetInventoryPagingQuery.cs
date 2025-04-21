using Shared.SeedWork.Paging;

namespace Shared.DTOs.Inventory;

public class GetInventoryPagingQuery : RequestParameters
{
    public string ItemNo { get; init; } = string.Empty;

    public string? SearchTerm { get; set; }

}