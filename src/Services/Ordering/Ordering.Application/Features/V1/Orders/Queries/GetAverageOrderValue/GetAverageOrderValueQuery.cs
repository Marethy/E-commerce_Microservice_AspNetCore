using MediatR;
using Ordering.Application.Common.Models;
using Shared.SeedWork.ApiResult;

namespace Ordering.Application.Features.V1.Orders.Queries.GetAverageOrderValue;

public class GetAverageOrderValueQuery : IRequest<ApiResult<List<AverageOrderValueDto>>>
{
    public int Days { get; set; } = 30;

    public GetAverageOrderValueQuery(int days = 30)
    {
        Days = days;
    }
}
