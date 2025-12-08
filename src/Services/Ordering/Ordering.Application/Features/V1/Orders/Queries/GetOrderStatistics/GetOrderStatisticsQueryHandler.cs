using MediatR;
using Ordering.Application.Common.Interfaces;
using Ordering.Application.Common.Models;
using Shared.SeedWork.ApiResult;

namespace Ordering.Application.Features.V1.Orders;

public class GetOrderStatisticsQueryHandler : IRequestHandler<GetOrderStatisticsQuery, ApiResult<OrderStatistics>>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderStatisticsQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<ApiResult<OrderStatistics>> Handle(GetOrderStatisticsQuery request, CancellationToken cancellationToken)
    {
        var stats = await _orderRepository.GetOrderStatisticsAsync();
        return new ApiSuccessResult<OrderStatistics>(stats);
    }
}
