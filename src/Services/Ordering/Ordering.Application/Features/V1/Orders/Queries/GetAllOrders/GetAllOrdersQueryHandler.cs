using Mapster;
using MediatR;
using Ordering.Application.Common.Interfaces;
using Ordering.Application.Common.Models;
using Shared.SeedWork.ApiResult;

namespace Ordering.Application.Features.V1.Orders;

public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, ApiResult<object>>
{
    private readonly IOrderRepository _orderRepository;

    public GetAllOrdersQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<ApiResult<object>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var (orders, totalCount) = await _orderRepository.GetAllOrdersAsync(request.Page, request.Limit, request.Status);
        var orderDtos = orders.Adapt<List<OrderDto>>();
        
        var result = new
        {
            orders = orderDtos,
            total = totalCount,
            totalPages = (int)Math.Ceiling(totalCount / (double)request.Limit),
            currentPage = request.Page
        };
        
        return new ApiSuccessResult<object>(result);
    }
}
