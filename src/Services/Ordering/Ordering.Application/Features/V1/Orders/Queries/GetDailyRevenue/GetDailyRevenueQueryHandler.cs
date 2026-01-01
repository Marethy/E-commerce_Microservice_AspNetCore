using MediatR;
using Ordering.Application.Common.Interfaces;
using Ordering.Application.Common.Models;
using Shared.SeedWork.ApiResult;

namespace Ordering.Application.Features.V1.Orders.Queries.GetDailyRevenue;

public class GetDailyRevenueQueryHandler : IRequestHandler<GetDailyRevenueQuery, ApiResult<List<DailyRevenueDto>>>
{
    private readonly IOrderRepository _orderRepository;

    public GetDailyRevenueQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<ApiResult<List<DailyRevenueDto>>> Handle(GetDailyRevenueQuery request, CancellationToken cancellationToken)
    {
        var dailyRevenue = await _orderRepository.GetDailyRevenueAsync(request.Days);
        return new ApiSuccessResult<List<DailyRevenueDto>>(dailyRevenue);
    }
}
