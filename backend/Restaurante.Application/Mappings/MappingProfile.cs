using AutoMapper;
using Restaurante.Application.DTOs;
using Restaurante.Domain.Entities;
using Restaurante.Domain.Enums;

namespace Restaurante.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(d => d.Role, o => o.MapFrom(s => s.Role.ToString()));

        CreateMap<Rider, RiderDto>()
            .ForMember(d => d.VehicleType, o => o.MapFrom(s => s.VehicleType.ToString()))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

        CreateMap<Restaurant, RestaurantDto>();
        CreateMap<Restaurant, RestaurantListDto>();
        CreateMap<CreateRestaurantDto, Restaurant>();
        CreateMap<UpdateRestaurantDto, Restaurant>();

        CreateMap<MenuItem, MenuItemDto>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category.Name));
        CreateMap<CreateMenuItemDto, MenuItem>();
        CreateMap<UpdateMenuItemDto, MenuItem>();

        CreateMap<Category, CategoryDto>();
        CreateMap<CreateCategoryDto, Category>();

        CreateMap<Order, OrderDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.PaymentStatus, o => o.MapFrom(s => s.PaymentStatus.ToString()))
            .ForMember(d => d.CustomerName, o => o.MapFrom(s => s.Customer.Name))
            .ForMember(d => d.RestaurantName, o => o.MapFrom(s => s.Restaurant.Name));

        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(d => d.MenuItemName, o => o.MapFrom(s => s.MenuItem.Name));

        CreateMap<AIConversation, AIConversationDto>();

        CreateMap<Payment, PaymentDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

        CreateMap<CustomerAddress, CustomerAddressDto>();

        CreateMap<Review, ReviewDto>()
            .ForMember(d => d.CustomerName, o => o.MapFrom(s => s.Customer.Name));

        CreateMap<Coupon, CouponDto>()
            .ForMember(d => d.DiscountType, o => o.MapFrom(s => s.DiscountType.ToString()));
    }
}
