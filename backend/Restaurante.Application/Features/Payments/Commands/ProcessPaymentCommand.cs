using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Exceptions;
using Restaurante.Application.Interfaces;
using Restaurante.Domain.Entities;
using Restaurante.Domain.Enums;

namespace Restaurante.Application.Features.Payments.Commands;

public class ProcessPaymentCommand : IRequest<ApiResponse<PaymentDto>>
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public string Method { get; set; } = string.Empty;
    public string? CardToken { get; set; }
    public string? AcceptanceToken { get; set; }
    public string? CustomerEmail { get; set; }
}

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, ApiResponse<PaymentDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentService _paymentService;
    private readonly IOrderNotifier _notifier;
    private readonly IMapper _mapper;

    public ProcessPaymentCommandHandler(
        IOrderRepository orderRepository,
        IPaymentRepository paymentRepository,
        IPaymentService paymentService,
        IOrderNotifier notifier,
        IMapper mapper)
    {
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _paymentService = paymentService;
        _notifier = notifier;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PaymentDto>> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId);
        if (order is null)
            throw new NotFoundException("Order not found");

        if (order.CustomerId != request.CustomerId)
            return ApiResponse<PaymentDto>.Fail("Order does not belong to this user");

        if (order.PaymentStatus != PaymentStatus.Pending)
            return ApiResponse<PaymentDto>.Fail("Order payment is not pending");

        var payment = (await _paymentRepository.GetByOrderIdAsync(order.Id))
            .FirstOrDefault(p => p.Status == PaymentStatus.Pending);
        if (payment is null)
        {
            payment = new Payment(order.Id, order.Total, request.Method);
            await _paymentRepository.AddAsync(payment);
        }
        else
        {
            payment.Amount = order.Total;
            payment.Method = request.Method;
            payment.UpdatedAt = DateTime.UtcNow;
        }

        var result = await _paymentService.ProcessPaymentAsync(order, new ProcessPaymentDto
        {
            Method = request.Method,
            CardToken = request.CardToken,
            AcceptanceToken = request.AcceptanceToken,
            CustomerEmail = request.CustomerEmail
        });

        payment.TransactionId = result.TransactionId;
        payment.Reference = result.Reference;
        if (Enum.TryParse<PaymentStatus>(result.Status, true, out var paymentStatus))
            payment.Status = paymentStatus;
        payment.UpdatedAt = DateTime.UtcNow;
        await _paymentRepository.UpdateAsync(payment);

        if (payment.Status == PaymentStatus.Paid)
        {
            order.PaymentStatus = PaymentStatus.Paid;
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepository.UpdateAsync(order);

            var orderDto = _mapper.Map<OrderDto>(order);
            await _notifier.NotifyOrderUpdated(order.RestaurantId, orderDto);
            await _notifier.NotifyOrderStatusChanged(order.Id, orderDto);
        }
        else if (payment.Status == PaymentStatus.Failed)
        {
            order.PaymentStatus = PaymentStatus.Failed;
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepository.UpdateAsync(order);
        }

        return ApiResponse<PaymentDto>.Ok(_mapper.Map<PaymentDto>(payment), result.Message);
    }
}
