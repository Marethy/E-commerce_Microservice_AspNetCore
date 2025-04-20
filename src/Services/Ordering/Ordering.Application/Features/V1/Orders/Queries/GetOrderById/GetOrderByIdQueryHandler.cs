using AutoMapper;
using MediatR;
using Ordering.Application.Common.Interfaces;
using Ordering.Application.Common.Models;
using Serilog;
using Shared.SeedWork;

namespace Ordering.Application.Features.V1.Orders;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, ApiResult<OrderDto>>
{
    private readonly ILogger _logger;
    private readonly IMapper _mapper;
    private readonly IOrderRepository _orderRepository;

    public GetOrderByIdQueryHandler(ILogger logger, IMapper mapper, IOrderRepository orderRepository)
    {
        _logger = logger;
        _mapper = mapper;
        _orderRepository = orderRepository;
    }

    private const string MethodName = "GetOrderByIdQueryHandler";

    public async Task<ApiResult<OrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.Information($"BEGIN: {MethodName} - Id: {request.Id}");

        var order = await _orderRepository.GetByIdAsync(request.Id);
        var orderDto = _mapper.Map<OrderDto>(order);

        _logger.Information($"END: {MethodName} - Id: {request.Id}");

        return new ApiSuccessResult<OrderDto>(orderDto);
    }
}