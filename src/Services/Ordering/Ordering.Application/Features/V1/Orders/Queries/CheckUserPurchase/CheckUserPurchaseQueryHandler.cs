using MediatR;
using Ordering.Application.Common.Interfaces;
using Shared.SeedWork.ApiResult;

namespace Ordering.Application.Features.V1.Orders;

public class CheckUserPurchaseQueryHandler : IRequestHandler<CheckUserPurchaseQuery, ApiResult<object>>
{
    private readonly IOrderRepository _orderRepository;

    public CheckUserPurchaseQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<ApiResult<object>> Handle(CheckUserPurchaseQuery request, CancellationToken cancellationToken)
    {
        var hasPurchased = await _orderRepository.HasUserPurchasedProductAsync(request.UserName, request.ProductNo);
        
        var result = new
        {
            hasPurchased = hasPurchased,
            userName = request.UserName,
            productNo = request.ProductNo
        };
        
        return new ApiSuccessResult<object>(result);
    }
}
