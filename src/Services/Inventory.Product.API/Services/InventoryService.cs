using AutoMapper;
using Infrastructure.Common;
using Inventory.Product.API.Entities;
using Inventory.Product.API.Extensions;
using Inventory.Product.API.Repositories;
using Inventory.Product.API.Services.Interfaces;
using MongoDB.Driver;
using Shared.Configurations;
using Shared.DTOs.Inventory;

namespace Inventory.Product.API.Services;

public class InventoryService : MongoDbRepository<InventoryEntry>, IInventoryService
{
    private readonly IMapper _mapper;

    public InventoryService(IMongoClient client, MongoDbSettings settings, IMapper mapper) : base(client, settings)
    {
        _mapper = mapper;
    }

    public async Task<IEnumerable<InventoryEntryDto>> GetAllByItemNoAsync(string itemNo)
    {
        var entities = await FindAll()
            .Find(x => x.ItemNo.Equals(itemNo))
            .ToListAsync();
        var result = _mapper.Map<IEnumerable<InventoryEntryDto>>(entities);

        return result;
    }

    public async Task<PagedList<InventoryEntryDto>> GetAllByItemNoPagingAsync(GetInventoryPagingQuery query)
    {
        var filterItemNo = Builders<InventoryEntry>.Filter.Eq(s => s.ItemNo, query.GetItemNo());

        var filterSearchTerm = Builders<InventoryEntry>.Filter.Empty;
        if (!string.IsNullOrEmpty(query.SearchTerm))
        { 
            filterSearchTerm = Builders<InventoryEntry>.Filter.Eq(s => s.DocumentNo, query.SearchTerm);
        }

        var andFilter = filterItemNo & filterSearchTerm;

        var pagedList = await PagedList<InventoryEntry>.ToPagedList(Collection, andFilter, query.PageIndex, query.PageSize);
        var items = _mapper.Map<IEnumerable<InventoryEntryDto>>(pagedList);
        var result = new PagedList<InventoryEntryDto>(items, pagedList.GetMetaData().TotalItems, query.PageIndex, query.PageSize);
        return result;
    }

    public async Task<InventoryEntryDto> GetInventoryByIdAsync(string id)
    {
        var entity = await FindAll()
            .Find(x => x.Id.Equals(id))
            .FirstOrDefaultAsync();
        var result = _mapper.Map<InventoryEntryDto>(entity);

        return result;
    }

    public async Task<InventoryEntryDto> PurchaseItemAsync(string itemNo, PurchaseProductDto model)
    {
        var itemToAdd = new InventoryEntry()
        {
            ItemNo = itemNo,
            Quantity = model.Quantity,
            DocumentType = model.DocumentType
        };
        await CreateAsync(itemToAdd);
        var result = _mapper.Map<InventoryEntryDto>(itemToAdd);
        return result;
    }

    public async Task<InventoryEntryDto> SalesItemAsync(string itemNo, SalesProductDto model)
    {
        var itemToAdd = new InventoryEntry()
        {
            ItemNo = itemNo,
            Quantity = model.Quantity * -1,
            DocumentType = model.DocumentType,
            ExternalDocumentNo = model.ExternalDocumentNo

        };
        await CreateAsync(itemToAdd);
        var result = _mapper.Map<InventoryEntryDto>(itemToAdd);
        return result;
    }

    public async Task<string> SalesOrderAsync(SalesOrderDto model)
    {
        var documentNo = Guid.NewGuid().ToString();
        foreach (var saleItem in model.SaleItems)
        {
            var itemToAdd = new InventoryEntry()
            {
                ItemNo = saleItem.ItemNo,
                Quantity = saleItem.Quantity * -1,
                DocumentType = saleItem.DocumentType,
                ExternalDocumentNo = model.OrderNo,
                DocumentNo = documentNo

            };
            await CreateAsync(itemToAdd);
        }

        return documentNo;
    }

    public async Task DeleteByDocumentNoAsync(string documentNo)
    {
        FilterDefinition<InventoryEntry> filter = Builders<InventoryEntry>.Filter.Eq(s => s.DocumentNo, documentNo);
        await Collection.DeleteManyAsync(filter);
    }
}