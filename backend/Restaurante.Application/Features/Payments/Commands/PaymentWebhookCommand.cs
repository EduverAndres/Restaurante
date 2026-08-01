using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Interfaces;
using Restaurante.Domain.Enums;

namespace Restaurante.Application.Features.Payments.Commands;

public class PaymentWebhookCommand : IRequest<ApiResponse<bool>>
{
    public string RawBody { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
    public PaymentWebhookDto Payload { get; set; } = new();
}

public class PaymentWebhookCommandHandler : IRequestHandler<PaymentWebhookCommand, ApiResponse<bool>>
{
    private readonly IPaymentService _paymentService;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderNotifier _notifier;
    private readonly IMapper _mapper;

    public PaymentWebhookCommandHandler(
        IPaymentService paymentService,
        IPaymentRepository paymentRepository,
        IOrderRepository orderRepository,
        IOrderNotifier notifier,
        IMapper mapper)
    {
        _paymentService = paymentService;
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _notifier = notifier;
        _mapper = mapper;
    }

    public async Task<ApiResponse<bool>> Handle(PaymentWebhookCommand request, CancellationToken cancellationToken)
    {
        if (!await _paymentService.VerifyWebhookSignatureAsync(request.RawBody, request.Signature))
            throw new UnauthorizedAccessException("Invalid webhook signature");

        if (!string.Equals(request.Payload.Event, "transaction.updated", StringComparison.OrdinalIgnoreCase))
            return ApiResponse<bool>.Ok(true);

        var data = request.Payload.Data;
        var payment = await _paymentRepository.GetByTransactionIdAsync(data.Id);
        if (payment is null)
            return ApiResponse<bool>.Ok(true);

        var newStatus = data.Status.ToUpperInvariant() switch
        {
            "APPROVED" => PaymentStatus.Paid,
            "DECLINED" or "ERROR" or "VOIDED" => PaymentStatus.Failed,
            _ => (PaymentStatus?)null
        };
        if (newStatus is null)
            return ApiResponse<bool>.Ok(true);

        payment.Status = newStatus.Value;
        if (string.IsNullOrEmpty(payment.Reference))
            payment.Reference = string.IsNullOrEmpty(data.Reference) ? payment.Reference : data.Reference;
        payment.UpdatedAt = DateTime.UtcNow;
        await _paymentRepository.UpdateAsync(payment);

        var order = await _orderRepository.GetByIdAsync(payment.OrderId);
        if (order is not null)
        {
            order.PaymentStatus = newStatus.Value;
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepository.UpdateAsync(order);

            var orderDto = _mapper.Map<OrderDto>(order);
            await _notifier.NotifyOrderUpdated(order.RestaurantId, orderDto);
            await _notifier.NotifyOrderStatusChanged(order.Id, orderDto);
        }

        return ApiResponse<bool>.Ok(true);
    }
}
