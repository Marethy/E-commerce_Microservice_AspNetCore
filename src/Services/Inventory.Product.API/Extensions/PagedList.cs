using Inventory.Product.API.Entities;
using Shared.SeedWork.Paging;

namespace Inventory.Product.API.Extensions
{
    public class PagedList<T> : List<T>
    {
        public MetaData MetaData { get; set; }

        public PagedList(List<T> items, int totalItems, int pageNumber, int pageSize)
        {
            MetaData = new MetaData
            {
                TotalItems = totalItems,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };
            AddRange(items);
        }

        public static PagedList<T> ToPagedList(MongoDB.Driver.IMongoCollection<InventoryEntry> collection, IQueryable<T> source, int pageNumber, int pageSize)
        {
            var totalItems = source.Count();
            var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return new PagedList<T>(items, totalItems, pageNumber, pageSize);
        }

       
    }
}