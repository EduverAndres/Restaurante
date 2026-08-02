using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Exceptions;
using Restaurante.Application.Interfaces;
using Restaurante.Domain.Enums;

namespace Restaurante.Application.Features.Restaurants.Queries;

public class GetRestaurantDashboardQuery : IRequest<ApiResponse<DashboardDto>>
{
    public Guid RestaurantId { get; set; }
    public Guid UserId { get; set; }
}

public class DashboardDto
{
    public decimal SalesToday { get; set; }
    public decimal SalesThisWeek { get; set; }
    public decimal SalesThisMonth { get; set; }
    public Dictionary<string, int> OrderCountsByStatus { get; set; } = new();
    public List<TopProductDto> TopProducts { get; set; } = new();
    public double? AveragePrepTimeMinutes { get; set; }
    public List<OrderDto> RecentOrders { get; set; } = new();
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class TopProductDto
{
    public Guid MenuItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Revenue { get; set; }
}

public class GetRestaurantDashboardQueryHandler : IRequestHandler<GetRestaurantDashboardQuery, ApiResponse<DashboardDto>>
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetRestaurantDashboardQueryHandler(
        IRestaurantRepository restaurantRepository,
        IOrderRepository orderRepository,
        IMapper mapper)
    {
        _restaurantRepository = restaurantRepository;
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<DashboardDto>> Handle(GetRestaurantDashboardQuery request, CancellationToken cancellationToken)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(request.RestaurantId);
        if (restaurant is null || restaurant.OwnerId != request.UserId)
            throw new NotFoundException("Restaurant not found");

        var orders = await _orderRepository.GetByRestaurantWithDetailsAsync(request.RestaurantId);
        var now = DateTime.UtcNow;
        var delivered = orders.Where(o => o.Status == OrderStatus.Delivered).ToList();

        var todayStart = now.Date;
        var weekStart = now.AddDays(-7);
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var topProducts = delivered
            .SelectMany(o => o.Items)
            .GroupBy(i => i.MenuItemId)
            .Select(g => new TopProductDto
            {
                MenuItemId = g.Key,
                Name = g.First().MenuItem?.Name ?? "Unknown",
                Quantity = g.Sum(i => i.Quantity),
                Revenue = g.Sum(i => i.UnitPrice * i.Quantity)
            })
            .OrderByDescending(p => p.Quantity)
            .Take(5)
            .ToList();

        var prepDiffs = new List<double>();
        foreach (var order in orders)
        {
            var preparingEntry = order.StatusHistory.FirstOrDefault(h => h.ToStatus == OrderStatus.Preparing);
            var readyEntry = order.StatusHistory.FirstOrDefault(h => h.ToStatus == OrderStatus.Ready);
            if (preparingEntry is not null && readyEntry is not null)
                prepDiffs.Add((readyEntry.CreatedAt - preparingEntry.CreatedAt).TotalMinutes);
        }

        var dto = new DashboardDto
        {
            SalesToday = delivered.Where(o => o.CreatedAt >= todayStart).Sum(o => o.Total),
            SalesThisWeek = delivered.Where(o => o.CreatedAt >= weekStart).Sum(o => o.Total),
            SalesThisMonth = delivered.Where(o => o.CreatedAt >= monthStart).Sum(o => o.Total),
            OrderCountsByStatus = Enum.GetValues<OrderStatus>()
                .ToDictionary(s => s.ToString(), s => orders.Count(o => o.Status == s)),
            TopProducts = topProducts,
            AveragePrepTimeMinutes = prepDiffs.Count > 0 ? prepDiffs.Average() : null,
            RecentOrders = _mapper.Map<List<OrderDto>>(orders.Take(10)),
            TotalOrders = orders.Count,
            TotalRevenue = delivered.Sum(o => o.Total)
        };

        return ApiResponse<DashboardDto>.Ok(dto);
    }
}
