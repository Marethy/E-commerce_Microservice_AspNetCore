using MediatR;
using Shared.SeedWork.ApiResult;

namespace Ordering.Application.Features.V1.Orders;

public class CheckUserPurchaseQuery : IRequest<ApiResult<object>>
{
    public string UserName { get; }
    public string ProductNo { get; }

    public CheckUserPurchaseQuery(string userName, string productNo)
    {
        UserName = userName;
        ProductNo = productNo;
    }
}
