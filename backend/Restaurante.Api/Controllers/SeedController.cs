using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurante.Application.Interfaces;
using Restaurante.Domain.Entities;
using Restaurante.Domain.Enums;

namespace Restaurante.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class SeedController : ControllerBase
{
    private readonly IUserRepository _users;
    private readonly IRestaurantRepository _restaurants;
    private readonly ICategoryRepository _categories;
    private readonly IMenuItemRepository _menuItems;
    private readonly IBusinessHourRepository _businessHours;
    private readonly ICouponRepository _coupons;
    private readonly ICustomerAddressRepository _addresses;
    private readonly IOrderRepository _orders;
    private readonly IRiderRepository _riders;
    private readonly IReviewRepository _reviews;
    private readonly IPasswordService _password;

    public SeedController(
        IUserRepository users, IRestaurantRepository restaurants,
        ICategoryRepository categories, IMenuItemRepository menuItems,
        IBusinessHourRepository businessHours, ICouponRepository coupons,
        ICustomerAddressRepository addresses, IOrderRepository orders,
        IRiderRepository riders, IReviewRepository reviews,
        IPasswordService password)
    {
        _users = users;
        _restaurants = restaurants;
        _categories = categories;
        _menuItems = menuItems;
        _businessHours = businessHours;
        _coupons = coupons;
        _addresses = addresses;
        _orders = orders;
        _riders = riders;
        _reviews = reviews;
        _password = password;
    }

    [HttpPost("seed")]
    public async Task<IActionResult> Seed()
    {
        var existing = await _users.GetByEmailAsync("demo@restaurante.app");
        var phase1Message = "Seed data created successfully";
        var restaurantsCreated = 0;
        var usersCreated = 0;
        var ridersCreated = 0;
        var couponsCreated = 0;
        var demoOrderDelivered = false;
        var demoCouponCode = "";

        if (existing != null)
        {
            phase1Message = "Seed data already exists";
        }
        else
        {
        // Demo owner
        var owner = new User("demo@restaurante.app", "Chef Carlos", _password.Hash("Demo123!"), UserRole.RestaurantOwner);
        await _users.AddAsync(owner);

        // Demo customer
        var customer = new User("cliente@restaurante.app", "María García", _password.Hash("Demo123!"), UserRole.Customer);
        await _users.AddAsync(customer);

        // === 1. LA CASA DEL TACO ===
        var taco = new Restaurant("La Casa del Taco", "la-casa-del-taco", owner.Id)
        {
            Description = "Auténtica cocina mexicana con recetas tradicionales. Tacos, quesadillas y más preparados al momento.",
            Logo = "https://images.unsplash.com/photo-1551504734-5ee1c4a1479b?w=200&h=200&fit=crop",
            CoverImage = "https://images.unsplash.com/photo-1565299585323-38d6b0865b47?w=1200&h=600&fit=crop",
            Phone = "+52 555 123 4567",
            Address = "Av. Reforma 123, Ciudad de México",
            Latitude = 19.4326,
            Longitude = -99.1332,
            RadiusKm = 6,
            DeliveryFee = 45,
            MinOrderAmount = 80,
            EstimatedPrepTimeMinutes = 20,
            ThemeConfig = "{\"primaryColor\":\"#d4852a\",\"secondaryColor\":\"#8c4f1d\",\"accentColor\":\"#e8bb7d\",\"backgroundColor\":\"#fdf8f0\",\"textColor\":\"#1a1a2e\",\"fontFamily\":\"Inter\"}"
        };
        await _restaurants.AddAsync(taco);

        var tacoCat1 = new Category("Tacos", taco.Id) { Description = "Tacos tradicionales", SortOrder = 1 };
        var tacoCat2 = new Category("Quesadillas", taco.Id) { Description = "Quesadillas doraditas", SortOrder = 2 };
        var tacoCat3 = new Category("Bebidas", taco.Id) { Description = "Refrescantes", SortOrder = 3 };
        await _categories.AddAsync(tacoCat1);
        await _categories.AddAsync(tacoCat2);
        await _categories.AddAsync(tacoCat3);

        var tacoAlPastor = new MenuItem("Tacos al Pastor", 89, taco.Id, tacoCat1.Id) { Description = "Tortilla de maíz con carne al pastor, piña, cebolla y cilantro", PreparationTime = 10, Images = new string[] { "https://images.unsplash.com/photo-1551504734-5ee1c4a1479b?w=400&h=300&fit=crop" }, IsFeatured = true };
        await _menuItems.AddAsync(tacoAlPastor);
        await _menuItems.AddAsync(new MenuItem("Tacos de Carnitas", 99, taco.Id, tacoCat1.Id) { Description = "Tortilla de maíz con carnitas de cerdo, salsa verde y aguacate", PreparationTime = 12, Images = new string[] { "https://images.unsplash.com/photo-1551504734-5ee1c4a1479b?w=400&h=300&fit=crop" } });
        var quesadillaHuitlacoche = new MenuItem("Quesadilla de Huitlacoche", 79, taco.Id, tacoCat2.Id) { Description = "Quesadilla de maíz azul rellena de huitlacoche y queso Oaxaca", PreparationTime = 8, Images = new string[] { "https://images.unsplash.com/photo-1615361200141-f45040f367be?w=400&h=300&fit=crop" }, IsFeatured = true };
        await _menuItems.AddAsync(quesadillaHuitlacoche);
        await _menuItems.AddAsync(new MenuItem("Quesadilla de Champiñones", 69, taco.Id, tacoCat2.Id) { Description = "Quesadilla de harina con champiñones salteados y queso manchego", PreparationTime = 8, Images = new string[] { "https://images.unsplash.com/photo-1615361200141-f45040f367be?w=400&h=300&fit=crop" } });
        var aguaHorchata = new MenuItem("Agua de Horchata", 35, taco.Id, tacoCat3.Id) { Description = "Agua fresca de horchata con canela", PreparationTime = 2, Images = new string[] { "https://images.unsplash.com/photo-1544145945-f90425340c7e?w=400&h=300&fit=crop" } };
        await _menuItems.AddAsync(aguaHorchata);
        await _menuItems.AddAsync(new MenuItem("Jamaica", 30, taco.Id, tacoCat3.Id) { Description = "Agua de jamaica bien fría", PreparationTime = 2, Images = new string[] { "https://images.unsplash.com/photo-1544145945-f90425340c7e?w=400&h=300&fit=crop" } });

        // === 2. DA VINCI'S TABLE ===
        var italian = new Restaurant("Da Vinci's Table", "da-vincis-table", owner.Id)
        {
            Description = "Cocina italiana artesanal. Pastas frescas, pizzas al horno de leña y postres tradicionales.",
            Logo = "https://images.unsplash.com/photo-1574071318508-1cdbab80d002?w=200&h=200&fit=crop",
            CoverImage = "https://images.unsplash.com/photo-1555396273-367ea4eb4db5?w=1200&h=600&fit=crop",
            Phone = "+52 555 987 6543",
            Address = "Callejón del Artista 45, Roma Norte",
            Latitude = 19.4194,
            Longitude = -99.1624,
            RadiusKm = 5,
            DeliveryFee = 55,
            MinOrderAmount = 120,
            EstimatedPrepTimeMinutes = 30,
            ThemeConfig = "{\"primaryColor\":\"#c0392b\",\"secondaryColor\":\"#e74c3c\",\"accentColor\":\"#f39c12\",\"backgroundColor\":\"#fdf8f0\",\"textColor\":\"#2c3e50\",\"fontFamily\":\"Playfair Display\"}"
        };
        await _restaurants.AddAsync(italian);

        var itCat1 = new Category("Pastas", italian.Id) { Description = "Pastas frescas artesanales", SortOrder = 1 };
        var itCat2 = new Category("Pizzas", italian.Id) { Description = "Pizzas al horno de leña", SortOrder = 2 };
        var itCat3 = new Category("Postres", italian.Id) { Description = "Dulces tentaciones", SortOrder = 3 };
        await _categories.AddAsync(itCat1);
        await _categories.AddAsync(itCat2);
        await _categories.AddAsync(itCat3);

        await _menuItems.AddAsync(new MenuItem("Spaghetti Carbonara", 189, italian.Id, itCat1.Id) { Description = "Spaghetti con huevo, panceta, parmesano y pimienta negra", PreparationTime = 15, Images = new string[] { "https://images.unsplash.com/photo-1612874742237-6526221588e3?w=400&h=300&fit=crop" }, IsFeatured = true });
        await _menuItems.AddAsync(new MenuItem("Lasagna Bolognese", 219, italian.Id, itCat1.Id) { Description = "Lasagna de pasta fresca con ragú boloñés y bechamel", PreparationTime = 20, Images = new string[] { "https://images.unsplash.com/photo-1574894709920-11b28e7367e3?w=400&h=300&fit=crop" } });
        await _menuItems.AddAsync(new MenuItem("Pizza Margherita", 169, italian.Id, itCat2.Id) { Description = "Pizza clásica con tomate, mozzarella fresca y albahaca", PreparationTime = 15, Images = new string[] { "https://images.unsplash.com/photo-1574071318508-1cdbab80d002?w=400&h=300&fit=crop" }, IsFeatured = true });
        await _menuItems.AddAsync(new MenuItem("Pizza Prosciutto", 199, italian.Id, itCat2.Id) { Description = "Pizza con prosciutto, rúcula y parmesano en lasca", PreparationTime = 15, Images = new string[] { "https://images.unsplash.com/photo-1574071318508-1cdbab80d002?w=400&h=300&fit=crop" } });
        await _menuItems.AddAsync(new MenuItem("Tiramisú", 99, italian.Id, itCat3.Id) { Description = "Tiramisú tradicional con mascarpone y café espresso", PreparationTime = 5, Images = new string[] { "https://images.unsplash.com/photo-1571877227200-a0d98ea607e9?w=400&h=300&fit=crop" }, IsFeatured = true });
        await _menuItems.AddAsync(new MenuItem("Panna Cotta", 89, italian.Id, itCat3.Id) { Description = "Panna cotta con reducción de frutos rojos", PreparationTime = 5, Images = new string[] { "https://images.unsplash.com/photo-1571877227200-a0d98ea607e9?w=400&h=300&fit=crop" } });

        // === 3. SAKURA SUSHI BAR ===
        var sushi = new Restaurant("Sakura Sushi Bar", "sakura-sushi-bar", owner.Id)
        {
            Description = "Sushi y cocina japonesa de autor. Ingredientes frescos importados y técnicas tradicionales.",
            Logo = "https://images.unsplash.com/photo-1579871494447-9811cf80d66c?w=200&h=200&fit=crop",
            CoverImage = "https://images.unsplash.com/photo-1553621042-f6e147245754?w=1200&h=600&fit=crop",
            Phone = "+52 555 456 7890",
            Address = "Av. del Pacífico 234, Polanco",
            Latitude = 19.4320,
            Longitude = -99.1910,
            RadiusKm = 4,
            DeliveryFee = 60,
            MinOrderAmount = 150,
            EstimatedPrepTimeMinutes = 25,
            ThemeConfig = "{\"primaryColor\":\"#c62828\",\"secondaryColor\":\"#880e4f\",\"accentColor\":\"#ffd54f\",\"backgroundColor\":\"#fafafa\",\"textColor\":\"#1a1a2e\",\"fontFamily\":\"Inter\"}"
        };
        await _restaurants.AddAsync(sushi);

        var susCat1 = new Category("Rollos", sushi.Id) { Description = "Rollos artesanales", SortOrder = 1 };
        var susCat2 = new Category("Sashimi", sushi.Id) { Description = "Pescado fresco rebanado", SortOrder = 2 };
        var susCat3 = new Category("Bebidas", sushi.Id) { Description = "Bebidas tradicionales", SortOrder = 3 };
        await _categories.AddAsync(susCat1);
        await _categories.AddAsync(susCat2);
        await _categories.AddAsync(susCat3);

        await _menuItems.AddAsync(new MenuItem("Roll California", 129, sushi.Id, susCat1.Id) { Description = "Roll de cangrejo, aguacate y pepino envuelto en ajonjolí", PreparationTime = 10, Images = new string[] { "https://images.unsplash.com/photo-1579871494447-9811cf80d66c?w=400&h=300&fit=crop" }, IsFeatured = true });
        await _menuItems.AddAsync(new MenuItem("Roll Spicy Tuna", 149, sushi.Id, susCat1.Id) { Description = "Roll de atún picante con arroz y alga nori", PreparationTime = 10, Images = new string[] { "https://images.unsplash.com/photo-1579871494447-9811cf80d66c?w=400&h=300&fit=crop" } });
        await _menuItems.AddAsync(new MenuItem("Sashimi Salmón", 179, sushi.Id, susCat2.Id) { Description = "8 piezas de salmón fresco c/n cortado a mano", PreparationTime = 12, Images = new string[] { "https://images.unsplash.com/photo-1553621042-f6e147245754?w=400&h=300&fit=crop" }, IsFeatured = true });
        await _menuItems.AddAsync(new MenuItem("Sashimi Mixto", 219, sushi.Id, susCat2.Id) { Description = "Surtido de salmón, atún y hamachi", PreparationTime = 15, Images = new string[] { "https://images.unsplash.com/photo-1553621042-f6e147245754?w=400&h=300&fit=crop" } });
        await _menuItems.AddAsync(new MenuItem("Té Verde", 35, sushi.Id, susCat3.Id) { Description = "Té verde matcha ceremonial", PreparationTime = 3, Images = new string[] { "https://images.unsplash.com/photo-1556881286-fc6915169721?w=400&h=300&fit=crop" } });
        await _menuItems.AddAsync(new MenuItem("Sake Premium", 149, sushi.Id, susCat3.Id) { Description = "Sake Junmai Daiginjo, copa", PreparationTime = 2, Images = new string[] { "https://images.unsplash.com/photo-1556881286-fc6915169721?w=400&h=300&fit=crop" }, IsFeatured = true });

        // === BUSINESS HOURS (9-23 todos los días; martes cerrado en italiano, domingo cerrado en sushi) ===
        var tacoHours = Enumerable.Range(0, 7)
            .Select(day => new BusinessHour { RestaurantId = taco.Id, DayOfWeek = day, OpenTime = new TimeSpan(9, 0, 0), CloseTime = new TimeSpan(23, 0, 0) })
            .ToList();
        var italianHours = Enumerable.Range(0, 7)
            .Select(day => new BusinessHour
            {
                RestaurantId = italian.Id,
                DayOfWeek = day,
                OpenTime = new TimeSpan(12, 0, 0),
                CloseTime = new TimeSpan(23, 0, 0),
                IsClosed = day == (int)DayOfWeek.Tuesday
            })
            .ToList();
        var sushiHours = Enumerable.Range(0, 7)
            .Select(day => new BusinessHour
            {
                RestaurantId = sushi.Id,
                DayOfWeek = day,
                OpenTime = new TimeSpan(11, 0, 0),
                CloseTime = new TimeSpan(23, 0, 0),
                IsClosed = day == (int)DayOfWeek.Sunday
            })
            .ToList();
        await _businessHours.ReplaceAsync(taco.Id, tacoHours);
        await _businessHours.ReplaceAsync(italian.Id, italianHours);
        await _businessHours.ReplaceAsync(sushi.Id, sushiHours);

        // === RIDER DEMO (ubicado a ~400 m de La Casa del Taco) ===
        var riderUser = new User("rider@restaurante.app", "Pedro Torres", _password.Hash("Demo123!"), UserRole.Delivery);
        await _users.AddAsync(riderUser);
        var rider = new Rider
        {
            UserId = riderUser.Id,
            VehicleType = VehicleType.Motorcycle,
            Status = RiderStatus.Available,
            Latitude = 19.4350,
            Longitude = -99.1370,
            Rating = 4.8m,
            RatingsCount = 47
        };
        await _riders.AddAsync(rider);

        // === CUPONES DEMO ===
        var welcomeCoupon = new Coupon
        {
            Code = "WELCOME10",
            DiscountType = DiscountType.Percentage,
            DiscountValue = 10,
            RestaurantId = taco.Id,
            ValidFrom = DateTime.UtcNow.AddDays(-30),
            ValidUntil = DateTime.UtcNow.AddDays(30),
            MaxUses = 1000,
            MinOrderAmount = 80,
            IsActive = true
        };
        var expiredCoupon = new Coupon
        {
            Code = "TACOFEST25",
            DiscountType = DiscountType.Fixed,
            DiscountValue = 25,
            RestaurantId = taco.Id,
            ValidFrom = DateTime.UtcNow.AddDays(-60),
            ValidUntil = DateTime.UtcNow.AddDays(-1),
            MaxUses = 500,
            MinOrderAmount = 100,
            IsActive = true
        };
        var italianCoupon = new Coupon
        {
            Code = "TABLE15",
            DiscountType = DiscountType.Fixed,
            DiscountValue = 15,
            RestaurantId = italian.Id,
            ValidFrom = DateTime.UtcNow.AddDays(-15),
            ValidUntil = DateTime.UtcNow.AddDays(15),
            MaxUses = 300,
            MinOrderAmount = 130,
            IsActive = true
        };
        await _coupons.AddAsync(welcomeCoupon);
        await _coupons.AddAsync(expiredCoupon);
        await _coupons.AddAsync(italianCoupon);

        // === DIRECCIÓN DEL CLIENTE DEMO ===
        var homeAddress = new CustomerAddress
        {
            UserId = customer.Id,
            Label = "Casa",
            Address = "Av. Reforma 123, Departamento 4, Ciudad de México",
            Latitude = 19.4300,
            Longitude = -99.1300,
            IsDefault = true
        };
        await _addresses.AddAsync(homeAddress);

        // === PEDIDO DEMO ENTREGADO (historial completo, pago CASH, cupón aplicado, reseña) ===
        var now = DateTime.UtcNow;
        var created = now.AddMinutes(-120);
        var subtotal = tacoAlPastor.Price * 2 + quesadillaHuitlacoche.Price + aguaHorchata.Price; // 89*2 + 79 + 35 = 292
        var preDiscountTotal = subtotal + taco.DeliveryFee; // 337
        var discount = Math.Round(preDiscountTotal * welcomeCoupon.DiscountValue / 100, 2); // 33.70
        welcomeCoupon.TimesUsed = 1;

        var demoOrder = new Order(customer.Id, taco.Id)
        {
            Status = OrderStatus.Delivered,
            Total = preDiscountTotal - discount,
            DeliveryFee = taco.DeliveryFee,
            DiscountAmount = discount,
            CouponId = welcomeCoupon.Id,
            PaymentStatus = PaymentStatus.Paid,
            DeliveryAddress = homeAddress.Address,
            Latitude = homeAddress.Latitude,
            Longitude = homeAddress.Longitude,
            RiderId = rider.Id,
            AssignedAt = created.AddMinutes(50),
            PickedUpAt = created.AddMinutes(60),
            DeliveredAt = created.AddMinutes(75),
            Notes = null,
            CreatedAt = created,
            UpdatedAt = created.AddMinutes(75)
        };
        demoOrder.Items.Add(new OrderItem(demoOrder.Id, tacoAlPastor.Id, 2, tacoAlPastor.Price) { CreatedAt = created });
        demoOrder.Items.Add(new OrderItem(demoOrder.Id, quesadillaHuitlacoche.Id, 1, quesadillaHuitlacoche.Price) { CreatedAt = created });
        demoOrder.Items.Add(new OrderItem(demoOrder.Id, aguaHorchata.Id, 1, aguaHorchata.Price) { CreatedAt = created });

        var transitionTimes = new (OrderStatus From, OrderStatus To, int Min)[]
        {
            (OrderStatus.Pending, OrderStatus.Confirmed, 10),
            (OrderStatus.Confirmed, OrderStatus.Preparing, 20),
            (OrderStatus.Preparing, OrderStatus.Ready, 35),
            (OrderStatus.Ready, OrderStatus.AssignedToRider, 50),
            (OrderStatus.AssignedToRider, OrderStatus.OutForDelivery, 60),
            (OrderStatus.OutForDelivery, OrderStatus.Delivered, 75)
        };
        foreach (var (from, to, minute) in transitionTimes)
        {
            demoOrder.StatusHistory.Add(new OrderStatusHistory(demoOrder.Id, from, to, "demo-seed")
            {
                CreatedAt = created.AddMinutes(minute)
            });
        }

        var cashPayment = new Payment(demoOrder.Id, demoOrder.Total, "CASH")
        {
            Status = PaymentStatus.Paid,
            TransactionId = $"CASH-{Guid.NewGuid():N}"[..20],
            CreatedAt = created.AddMinutes(5)
        };
        demoOrder.Payments.Add(cashPayment);

        await _orders.AddAsync(demoOrder);
        await _coupons.UpdateAsync(welcomeCoupon);

        var demoReview = new Review
        {
            RestaurantId = taco.Id,
            CustomerId = customer.Id,
            OrderId = demoOrder.Id,
            Rating = 5,
            Comment = "¡Los tacos al pastor estaban increíbles y llegaron súper rápido! La quesadilla de huitlacoche es imperdible.",
            CreatedAt = created.AddMinutes(80)
        };
        await _reviews.AddAsync(demoReview);

            restaurantsCreated = 3;
            usersCreated = 3;
            ridersCreated = 1;
            couponsCreated = 3;
            demoOrderDelivered = true;
            demoCouponCode = welcomeCoupon.Code;
        }

        var extended = await SeedExtendedDataAsync();
        var extendedMessage = extended.Orders > 0
            ? $"Extended seed created: {extended.Orders} orders, {extended.Reviews} reviews, {extended.Coupons} coupons"
            : "Extended seed already exists";

        return Ok(new
        {
            message = phase1Message,
            extendedMessage,
            demoEmail = "demo@restaurante.app",
            customerEmail = "cliente@restaurante.app",
            riderEmail = "rider@restaurante.app",
            demoPassword = "Demo123!",
            restaurantsCreated,
            usersCreated,
            ridersCreated,
            couponsCreated,
            demoOrderDelivered,
            demoCoupon = demoCouponCode
        });
    }

    /// <summary>
    /// Segundo set idempotente de datos "vivos": pedidos en todos los estados del ciclo
    /// (pending, preparing, ready, out-for-delivery, delivered y cancelled), reseñas en
    /// más restaurantes y un cupón extra. Guardado por el cupón marcador "SEEDV2" para que
    /// correr /api/seed varias veces no duplique los pedidos (los pedidos pueden cambiar de
    /// estado durante la validación manual, por eso el marcador no depende del estado).
    /// </summary>
    private async Task<(int Orders, int Reviews, int Coupons)> SeedExtendedDataAsync()
    {
        if (await _coupons.GetByCodeAsync("SEEDV2") != null)
            return (0, 0, 0);

        var customer = await _users.GetByEmailAsync("cliente@restaurante.app");
        var riderUser = await _users.GetByEmailAsync("rider@restaurante.app");
        var taco = await _restaurants.GetBySlugAsync("la-casa-del-taco");
        var italian = await _restaurants.GetBySlugAsync("da-vincis-table");
        var sushi = await _restaurants.GetBySlugAsync("sakura-sushi-bar");
        if (customer is null || riderUser is null || taco is null || italian is null || sushi is null)
            return (0, 0, 0);

        var rider = await _riders.GetByUserIdAsync(riderUser.Id);
        var home = (await _addresses.GetByUserIdAsync(customer.Id)).FirstOrDefault()?.Address
            ?? "Av. Reforma 123, Ciudad de México";
        if (rider is null)
            return (0, 0, 0);

        try
        {
            var tacoMenu = await _menuItems.GetByRestaurantIdAsync(taco.Id);
            var italianMenu = await _menuItems.GetByRestaurantIdAsync(italian.Id);
            var sushiMenu = await _menuItems.GetByRestaurantIdAsync(sushi.Id);

            var pastor = MenuItemByName(tacoMenu, "Tacos al Pastor");
            var huitlacoche = MenuItemByName(tacoMenu, "Quesadilla de Huitlacoche");
            var horchata = MenuItemByName(tacoMenu, "Agua de Horchata");
            var jamaica = MenuItemByName(tacoMenu, "Jamaica");
            var spaghetti = MenuItemByName(italianMenu, "Spaghetti Carbonara");
            var margherita = MenuItemByName(italianMenu, "Pizza Margherita");
            var prosciutto = MenuItemByName(italianMenu, "Pizza Prosciutto");
            var california = MenuItemByName(sushiMenu, "Roll California");
            var teVerde = MenuItemByName(sushiMenu, "Té Verde");
            var sashimi = MenuItemByName(sushiMenu, "Sashimi Salmón");
            var sake = MenuItemByName(sushiMenu, "Sake Premium");

            var now = DateTime.UtcNow;
            var orders = 0;

            // 1) Pending en Da Vinci's (sin cupón: sirve para probar POST apply-coupon).
            var pendingItalian = OrderAt(now.AddMinutes(-12), customer, italian, home, OrderStatus.Pending, PaymentStatus.Pending,
                "Sin cebolla en la pizza, por favor", now.AddMinutes(-12));
            pendingItalian.Items.Add(new OrderItem(pendingItalian.Id, spaghetti.Id, 1, spaghetti.Price) { CreatedAt = now.AddMinutes(-12) });
            pendingItalian.Items.Add(new OrderItem(pendingItalian.Id, margherita.Id, 1, margherita.Price) { CreatedAt = now.AddMinutes(-12) });
            await _orders.AddAsync(pendingItalian);
            orders++;

            // 2) Preparing en Sakura (pago CARD).
            var preparingSushi = OrderAt(now.AddMinutes(-42), customer, sushi, home, OrderStatus.Preparing, PaymentStatus.Paid, null, now.AddMinutes(-15));
            preparingSushi.Items.Add(new OrderItem(preparingSushi.Id, california.Id, 2, california.Price) { CreatedAt = now.AddMinutes(-42) });
            preparingSushi.Items.Add(new OrderItem(preparingSushi.Id, teVerde.Id, 1, teVerde.Price) { CreatedAt = now.AddMinutes(-42) });
            preparingSushi.StatusHistory.Add(new OrderStatusHistory(preparingSushi.Id, OrderStatus.Pending, OrderStatus.Confirmed, "demo-seed") { CreatedAt = now.AddMinutes(-25) });
            preparingSushi.StatusHistory.Add(new OrderStatusHistory(preparingSushi.Id, OrderStatus.Confirmed, OrderStatus.Preparing, "demo-seed") { CreatedAt = now.AddMinutes(-15) });
            preparingSushi.Payments.Add(new Payment(preparingSushi.Id, preparingSushi.Total, "CARD")
            {
                Status = PaymentStatus.Paid,
                TransactionId = $"TXN-SEED-{Guid.NewGuid():N}"[..20],
                CreatedAt = now.AddMinutes(-40)
            });
            await _orders.AddAsync(preparingSushi);
            orders++;

            // 3) Ready en La Casa del Taco (pago CARD).
            var readyTaco = OrderAt(now.AddMinutes(-60), customer, taco, home, OrderStatus.Ready, PaymentStatus.Paid, null, now.AddMinutes(-10));
            readyTaco.Items.Add(new OrderItem(readyTaco.Id, pastor.Id, 1, pastor.Price) { CreatedAt = now.AddMinutes(-60) });
            readyTaco.Items.Add(new OrderItem(readyTaco.Id, horchata.Id, 1, horchata.Price) { CreatedAt = now.AddMinutes(-60) });
            AddHistory(readyTaco, 45, 30, 10);
            readyTaco.Payments.Add(new Payment(readyTaco.Id, readyTaco.Total, "CARD")
            {
                Status = PaymentStatus.Paid,
                TransactionId = $"TXN-SEED-{Guid.NewGuid():N}"[..20],
                CreatedAt = now.AddMinutes(-58)
            });
            await _orders.AddAsync(readyTaco);
            orders++;

            // 4) OutForDelivery en Taco con rider asignado (los pedidos vivos del rider).
            var outForDeliveryTaco = OrderAt(now.AddMinutes(-90), customer, taco, home, OrderStatus.OutForDelivery, PaymentStatus.Paid, null, now.AddMinutes(-25));
            outForDeliveryTaco.Items.Add(new OrderItem(outForDeliveryTaco.Id, huitlacoche.Id, 2, huitlacoche.Price) { CreatedAt = now.AddMinutes(-90) });
            outForDeliveryTaco.Items.Add(new OrderItem(outForDeliveryTaco.Id, jamaica.Id, 1, jamaica.Price) { CreatedAt = now.AddMinutes(-90) });
            AddHistory(outForDeliveryTaco, 80, 65, 50, 35, 25);
            outForDeliveryTaco.RiderId = rider.Id;
            outForDeliveryTaco.AssignedAt = now.AddMinutes(-35);
            outForDeliveryTaco.PickedUpAt = now.AddMinutes(-25);
            outForDeliveryTaco.Payments.Add(new Payment(outForDeliveryTaco.Id, outForDeliveryTaco.Total, "CARD")
            {
                Status = PaymentStatus.Paid,
                TransactionId = $"TXN-SEED-{Guid.NewGuid():N}"[..20],
                CreatedAt = now.AddMinutes(-88)
            });
            await _orders.AddAsync(outForDeliveryTaco);
            orders++;

            // 5) Cancelled en Da (para ver el estado y el history Pending → Cancelled).
            var cancelItalian = OrderAt(now.AddMinutes(-30), customer, italian, home, OrderStatus.Cancelled, PaymentStatus.Pending,
                "Cliente canceló antes de confirmar", now.AddMinutes(-5));
            cancelItalian.Items.Add(new OrderItem(cancelItalian.Id, prosciutto.Id, 1, prosciutto.Price) { CreatedAt = now.AddMinutes(-30) });
            cancelItalian.StatusHistory.Add(new OrderStatusHistory(cancelItalian.Id, OrderStatus.Pending, OrderStatus.Cancelled, "demo-seed") { CreatedAt = now.AddMinutes(-5) });
            await _orders.AddAsync(cancelItalian);
            orders++;

            // 6) Delivered en Sakura con reseña (rating 4).
            var deliveredSushi = OrderAt(now.AddMinutes(-150), customer, sushi, home, OrderStatus.Delivered, PaymentStatus.Paid, null, now.AddMinutes(-60));
            deliveredSushi.Items.Add(new OrderItem(deliveredSushi.Id, sashimi.Id, 2, sashimi.Price) { CreatedAt = now.AddMinutes(-150) });
            deliveredSushi.Items.Add(new OrderItem(deliveredSushi.Id, sake.Id, 1, sake.Price) { CreatedAt = now.AddMinutes(-150) });
            AddHistory(deliveredSushi, 130, 115, 100, 85, 75, 60);
            deliveredSushi.RiderId = rider.Id;
            deliveredSushi.AssignedAt = now.AddMinutes(-85);
            deliveredSushi.PickedUpAt = now.AddMinutes(-75);
            deliveredSushi.DeliveredAt = now.AddMinutes(-60);
            deliveredSushi.Payments.Add(new Payment(deliveredSushi.Id, deliveredSushi.Total, "CASH")
            {
                Status = PaymentStatus.Paid,
                TransactionId = $"CASH-SEED-{Guid.NewGuid():N}"[..20],
                CreatedAt = now.AddMinutes(-148)
            });
            await _orders.AddAsync(deliveredSushi);
            orders++;

            // 7) Delivered en Da — V's Table con reseña (rating 5).
            var deliveredItalian = OrderAt(now.AddMinutes(-210), customer, italian, home, OrderStatus.Delivered, PaymentStatus.Paid, null, now.AddMinutes(-115));
            deliveredItalian.Items.Add(new OrderItem(deliveredItalian.Id, spaghetti.Id, 1, spaghetti.Price) { CreatedAt = now.AddMinutes(-210) });
            deliveredItalian.Items.Add(new OrderItem(deliveredItalian.Id, prosciutto.Id, 1, prosciutto.Price) { CreatedAt = now.AddMinutes(-210) });
            AddHistory(deliveredItalian, 190, 175, 160, 145, 130, 115);
            deliveredItalian.RiderId = rider.Id;
            deliveredItalian.AssignedAt = now.AddMinutes(-145);
            deliveredItalian.PickedUpAt = now.AddMinutes(-130);
            deliveredItalian.DeliveredAt = now.AddMinutes(-115);
            deliveredItalian.Payments.Add(new Payment(deliveredItalian.Id, deliveredItalian.Total, "CASH")
            {
                Status = PaymentStatus.Paid,
                TransactionId = $"CASH-SEED-{Guid.NewGuid():N}"[..20],
                CreatedAt = now.AddMinutes(-208)
            });
            await _orders.AddAsync(deliveredItalian);
            orders++;

            // Reseñas extra para que la agregación de rating de resto no esté vacía.
            await _reviews.AddAsync(new Review
            {
                RestaurantId = sushi.Id,
                CustomerId = customer.Id,
                OrderId = deliveredSushi.Id,
                Rating = 4,
                Comment = "Sushi súper fresco y muy bien presentado. El sashimi de salmón es imperdible.",
                CreatedAt = now.AddMinutes(-55)
            });
            await _reviews.AddAsync(new Review
            {
                RestaurantId = italian.Id,
                CustomerId = customer.Id,
                OrderId = deliveredItalian.Id,
                Rating = 5,
                Comment = "La carbonara es la mejor que he probado fuera de Roma. Pasta fresca de verdad.",
                CreatedAt = now.AddMinutes(-110)
            });
            var reviews = 2;

            // Cupón marcador + dato extra para el listado de cupones del owner.
            await _coupons.AddAsync(new Coupon
            {
                Code = "SEEDV2",
                DiscountType = DiscountType.Fixed,
                DiscountValue = 20,
                RestaurantId = sushi.Id,
                ValidFrom = DateTime.UtcNow.AddDays(-30),
                ValidUntil = DateTime.UtcNow.AddDays(30),
                MaxUses = 100,
                MinOrderAmount = 150,
                IsActive = true
            });
            var coupons = 1;

            return (orders, reviews, coupons);
        }
        catch (InvalidOperationException)
        {
            // Algún ítem del menú fase 1 no existe: no podemos armar pedidos válidos.
            return (0, 0, 0);
        }
    }

    private static MenuItem MenuItemByName(List<MenuItem> menu, string name) =>
        menu.First(i => i.Name == name);

    private static Order OrderAt(DateTime createdAt, User customer, Restaurant restaurant, string address,
        OrderStatus status, PaymentStatus paymentStatus, string? notes, DateTime updatedAt) =>
        new Order(customer.Id, restaurant.Id)
        {
            Status = status,
            PaymentStatus = paymentStatus,
            DeliveryAddress = address,
            Notes = notes,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

    private static void AddHistory(Order order, params double[] minutesFromNow)
    {
        var now = DateTime.UtcNow;
        var steps = new[] { OrderStatus.Pending, OrderStatus.Confirmed, OrderStatus.Preparing, OrderStatus.Ready, OrderStatus.AssignedToRider, OrderStatus.OutForDelivery, OrderStatus.Delivered };
        for (int i = 0; i < minutesFromNow.Length; i++)
            order.StatusHistory.Add(new OrderStatusHistory(order.Id, steps[i], steps[i + 1], "demo-seed") { CreatedAt = now.AddMinutes(-minutesFromNow[i]) });
    }
}
