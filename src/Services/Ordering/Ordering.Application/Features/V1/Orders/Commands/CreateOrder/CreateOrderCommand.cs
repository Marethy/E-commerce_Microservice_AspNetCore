using MediatR;
using Shared.SeedWork.ApiResult;

namespace Ordering.Application.Features.V1.Orders;

public class CreateOrderCommand : CreateOrUpdateCommand, IRequest<ApiResult<long>>
{
    public string UserName { get; set; }
}