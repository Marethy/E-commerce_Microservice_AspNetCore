using MediatR;
using Shared.SeedWork.ApiResult;

namespace Ordering.Application.Features.V1.Orders;

public class GetAllOrdersQuery : IRequest<ApiResult<object>>
{
    public int Page { get; }
    public int Limit { get; }
    public string? Status { get; }

    public GetAllOrdersQuery(int page, int limit, string? status = null)
    {
        Page = page;
        Limit = limit;
        Status = status;
    }
}
