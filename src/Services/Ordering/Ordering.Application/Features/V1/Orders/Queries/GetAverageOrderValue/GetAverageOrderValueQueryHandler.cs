using MediatR;
using Ordering.Application.Common.Interfaces;
using Ordering.Application.Common.Models;
using Shared.SeedWork.ApiResult;

namespace Ordering.Application.Features.V1.Orders.Queries.GetAverageOrderValue;

public class GetAverageOrderValueQueryHandler : IRequestHandler<GetAverageOrderValueQuery, ApiResult<List<AverageOrderValueDto>>>
{
    private readonly IOrderRepository _orderRepository;

    public GetAverageOrderValueQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<ApiResult<List<AverageOrderValueDto>>> Handle(GetAverageOrderValueQuery request, CancellationToken cancellationToken)
    {
        var avgOrderValue = await _orderRepository.GetAverageOrderValueAsync(request.Days);
        return new ApiSuccessResult<List<AverageOrderValueDto>>(avgOrderValue);
    }
}
