using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Exceptions;
using Restaurante.Application.Interfaces;
using Restaurante.Domain.Enums;

namespace Restaurante.Application.Features.Payments.Commands;

public class RefundPaymentCommand : IRequest<ApiResponse<PaymentDto>>
{
    public Guid PaymentId { get; set; }
}

public class RefundPaymentCommandHandler : IRequestHandler<RefundPaymentCommand, ApiResponse<PaymentDto>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentService _paymentService;
    private readonly IOrderNotifier _notifier;
    private readonly IMapper _mapper;

    public RefundPaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IOrderRepository orderRepository,
        IPaymentService paymentService,
        IOrderNotifier notifier,
        IMapper mapper)
    {
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _paymentService = paymentService;
        _notifier = notifier;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PaymentDto>> Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(request.PaymentId);
        if (payment is null)
            throw new NotFoundException("Payment not found");

        if (payment.Status != PaymentStatus.Paid)
            return ApiResponse<PaymentDto>.Fail("Only paid payments can be refunded");

        var result = await _paymentService.RefundPaymentAsync(payment);
        if (!result.Success)
            return ApiResponse<PaymentDto>.Fail(result.Message);

        payment.Status = PaymentStatus.Refunded;
        payment.UpdatedAt = DateTime.UtcNow;
        await _paymentRepository.UpdateAsync(payment);

        var order = await _orderRepository.GetByIdAsync(payment.OrderId);
        if (order is not null)
        {
            order.PaymentStatus = PaymentStatus.Refunded;
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepository.UpdateAsync(order);

            var orderDto = _mapper.Map<OrderDto>(order);
            await _notifier.NotifyOrderUpdated(order.RestaurantId, orderDto);
            await _notifier.NotifyOrderStatusChanged(order.Id, orderDto);
        }

        return ApiResponse<PaymentDto>.Ok(_mapper.Map<PaymentDto>(payment), "Payment refunded");
    }
}
