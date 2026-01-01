using MediatR;
using Ordering.Application.Common.Interfaces;
using Ordering.Application.Common.Models;
using Shared.SeedWork.ApiResult;

namespace Ordering.Application.Features.V1.Orders.Queries.GetRevenueByStatus;

public class GetRevenueByStatusQueryHandler : IRequestHandler<GetRevenueByStatusQuery, ApiResult<List<RevenueByStatusDto>>>
{
    private readonly IOrderRepository _orderRepository;

    public GetRevenueByStatusQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<ApiResult<List<RevenueByStatusDto>>> Handle(GetRevenueByStatusQuery request, CancellationToken cancellationToken)
    {
        var revenueByStatus = await _orderRepository.GetRevenueByStatusAsync();
        return new ApiSuccessResult<List<RevenueByStatusDto>>(revenueByStatus);
    }
}
