using AutoMapper;
using MediatR;
using Ordering.Application.Common.Interfaces;
using Ordering.Application.Common.Models;
using Shared.SeedWork.ApiResult;

namespace Ordering.Application.Features.V1.Orders;

public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, ApiResult<object>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetAllOrdersQueryHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<ApiResult<object>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var (orders, totalCount) = await _orderRepository.GetAllOrdersAsync(request.Page, request.Limit, request.Status);
        var orderDtos = _mapper.Map<List<OrderDto>>(orders);
        
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
