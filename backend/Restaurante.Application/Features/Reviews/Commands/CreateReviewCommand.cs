using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Exceptions;
using Restaurante.Application.Interfaces;
using Restaurante.Domain.Entities;
using Restaurante.Domain.Enums;

namespace Restaurante.Application.Features.Reviews.Commands;

public class CreateReviewCommand : IRequest<ApiResponse<ReviewDto>>
{
    public Guid RestaurantId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid OrderId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}

public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, ApiResponse<ReviewDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly IUserRepository _userRepository;

    public CreateReviewCommandHandler(IOrderRepository orderRepository, IReviewRepository reviewRepository, IUserRepository userRepository)
    {
        _orderRepository = orderRepository;
        _reviewRepository = reviewRepository;
        _userRepository = userRepository;
    }

    public async Task<ApiResponse<ReviewDto>> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId);
        if (order is null || order.CustomerId != request.CustomerId)
            return ApiResponse<ReviewDto>.Fail("Order not found");

        if (order.RestaurantId != request.RestaurantId)
            return ApiResponse<ReviewDto>.Fail("Order does not belong to this restaurant");

        if (order.Status != OrderStatus.Delivered)
            return ApiResponse<ReviewDto>.Fail("Only delivered orders can be reviewed");

        var existing = await _reviewRepository.GetByOrderIdAsync(request.OrderId);
        if (existing is not null)
            return ApiResponse<ReviewDto>.Fail("This order has already been reviewed");

        var review = new Review
        {
            RestaurantId = request.RestaurantId,
            CustomerId = request.CustomerId,
            OrderId = request.OrderId,
            Rating = request.Rating,
            Comment = request.Comment
        };

        await _reviewRepository.AddAsync(review);

        var customer = await _userRepository.GetByIdAsync(request.CustomerId);
        var dto = new ReviewDto
        {
            Id = review.Id,
            RestaurantId = review.RestaurantId,
            CustomerId = review.CustomerId,
            CustomerName = customer?.Name ?? string.Empty,
            OrderId = review.OrderId,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };

        return ApiResponse<ReviewDto>.Ok(dto, "Review created");
    }
}
