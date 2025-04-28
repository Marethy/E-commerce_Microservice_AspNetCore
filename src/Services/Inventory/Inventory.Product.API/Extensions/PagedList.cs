using MongoDB.Driver;
using Shared.SeedWork.Paging;

namespace Inventory.Product.API.Extensions;

public class PagedList<T> : List<T>
{
    private MetaData MetaData { get; }

    public MetaData GetMetaData() => MetaData;

    public PagedList(IEnumerable<T> items, long totalItems, int pageIndex, int pageSize)
    {
        MetaData = new MetaData
        {
            TotalItems = totalItems,
            PageSize = pageSize,
            CurrentPage = pageIndex,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
        };
        AddRange(items);
    }

    public static async Task<PagedList<T>> ToPagedList(IMongoCollection<T> source,
                                                       FilterDefinition<T> filter,
                                                       int pageIndex,
                                                       int pageSize)
    {
        var count = await source.Find(filter).CountDocumentsAsync();
        var items = await source.Find(filter)
            .Skip((pageIndex - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        return new PagedList<T>(items, count, pageIndex, pageSize);
    }
}