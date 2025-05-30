using AutoMapper;
using MediatR;
using Ordering.Application.Common.Interfaces;
using Ordering.Application.Common.Models;
using Serilog;
using Shared.SeedWork;
using Shared.SeedWork.ApiResult;

namespace Ordering.Application.Features.V1.Orders;

public class GetOrderByIdQueryHandler(ILogger logger, IMapper mapper, IOrderRepository orderRepository) : IRequestHandler<GetOrderByIdQuery, ApiResult<OrderDto>>
{
    private const string MethodName = "GetOrderByIdQueryHandler";

    public async Task<ApiResult<OrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        logger.Information($"BEGIN: {MethodName} - Id: {request.Id}");

        var order = await orderRepository.GetByIdAsync(request.Id);
        var orderDto = mapper.Map<OrderDto>(order);

        logger.Information($"END: {MethodName} - Id: {request.Id}");

        return new ApiSuccessResult<OrderDto>(orderDto);
    }
}