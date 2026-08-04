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
        var phase1Message = "Seed data created successfully";
        var restaurantsCreated = 0;
        var usersCreated = 0;
        var ridersCreated = 0;
        var couponsCreated = 0;
        var demoOrderDelivered = false;
        var demoCouponCode = "";

        // Owner demo: reusar si ya existe (evita duplicar al re-ejecutar seed).
        var owner = await _users.GetByEmailAsync("demo@restaurante.app");
        if (owner is null)
        {
            owner = new User("demo@restaurante.app", "Chef Carlos", _password.Hash("Demo123!"), UserRole.RestaurantOwner);
            await _users.AddAsync(owner);
            usersCreated++;
        }

        // Customer demo: reusar si ya existe.
        var customer = await _users.GetByEmailAsync("cliente@restaurante.app");
        if (customer is null)
        {
            customer = new User("cliente@restaurante.app", "María García", _password.Hash("Demo123!"), UserRole.Customer);
            await _users.AddAsync(customer);
            usersCreated++;
        }

        // Horse demo: reusar si ya existe.
        var riderUser = await _users.GetByEmailAsync("rider@restaurante.app");
        if (riderUser is null)
        {
            riderUser = new User("rider@restaurante.app", "Pedro Torres", _password.Hash("Demo123!"), UserRole.Delivery);
            await _users.AddAsync(riderUser);
            usersCreated++;
        }

        var tacoExists = await _restaurants.GetBySlugAsync("la-casa-del-taco");
        if (tacoExists is null)
        {
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

        var catalog = await SeedRealisticCatalogAsync();
        var catalogMessage = catalog.Restaurants > 0
            ? $"Realistic catalog created: {catalog.Restaurants} restaurants, {catalog.MenuItems} menu items, {catalog.Coupons} coupons, {catalog.Orders} orders, {catalog.Reviews} reviews"
            : "Realistic catalog already exists";

        return Ok(new
        {
            message = phase1Message,
            extendedMessage,
            catalogMessage,
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

    /// <summary>
    /// Catálogo realista adicional: 6 restaurantes con menús completos, horarios, cupones
    /// y pedidos entregados con reseñas. Idempotente: se salta si "La Burger House" existe.
    /// </summary>
    private async Task<(int Restaurants, int MenuItems, int Coupons, int Orders, int Reviews)> SeedRealisticCatalogAsync()
    {
        if (await _restaurants.GetBySlugAsync("la-burger-house") != null)
            return (0, 0, 0, 0, 0);

        var owner = await _users.GetByEmailAsync("demo@restaurante.app");
        var customer = await _users.GetByEmailAsync("cliente@restaurante.app");
        var riderUser = await _users.GetByEmailAsync("rider@restaurante.app");
        if (owner is null || customer is null || riderUser is null)
            return (0, 0, 0, 0, 0);

        var rider = await _riders.GetByUserIdAsync(riderUser.Id);
        if (rider is null)
            return (0, 0, 0, 0, 0);

        var home = (await _addresses.GetByUserIdAsync(customer.Id)).FirstOrDefault()?.Address
            ?? "Av. Reforma 123, Ciudad de México";

        var restaurants = 0;
        var menuItems = 0;
        var coupons = 0;
        var orders = 0;
        var reviews = 0;

        // === 1. LA BURGER HOUSE (hamburguesas, Roma Norte) ===
        var burger = new Restaurant("La Burger House", "la-burger-house", owner.Id)
        {
            Description = "Hamburguesas artesanales de res Angus con pan brioche, maduradas y a la leña. Papas caseras y malteadas de temporada.",
            Logo = "https://images.unsplash.com/photo-1571091718767-18b5b1457add?w=200&h=200&fit=crop",
            CoverImage = "https://images.unsplash.com/photo-1550547660-d9450f859349?w=1200&h=600&fit=crop",
            Phone = "+52 555 214 8890",
            Address = "Av. Álvaro Obregón 158, Roma Norte, CDMX",
            Latitude = 19.4189,
            Longitude = -99.1631,
            RadiusKm = 6,
            DeliveryFee = 49,
            MinOrderAmount = 120,
            EstimatedPrepTimeMinutes = 25,
            ThemeConfig = "{\"primaryColor\":\"#b2582b\",\"secondaryColor\":\"#7a3b1c\",\"accentColor\":\"#f2c14e\",\"backgroundColor\":\"#fdf6ee\",\"textColor\":\"#241c15\",\"fontFamily\":\"Inter\"}"
        };
        await _restaurants.AddAsync(burger);
        restaurants++;

        var burgerCat1 = new Category("Hamburguesas", burger.Id) { Description = "Artesanales", SortOrder = 1 };
        var burgerCat2 = new Category("Sides", burger.Id) { Description = "Guarniciones", SortOrder = 2 };
        var burgerCat3 = new Category("Bebidas", burger.Id) { Description = "Refrescos y malteadas", SortOrder = 3 };
        await _categories.AddAsync(burgerCat1);
        await _categories.AddAsync(burgerCat2);
        await _categories.AddAsync(burgerCat3);

        var burgerSmash = await AddMenuItemAsync(burger.Id, burgerCat1.Id, "Smash Burger Clásica", 165,
            "Doble smash de res, queso americano, cebolla confitada, lechuga y salsa de la casa en pan brioche.", 12, "https://images.unsplash.com/photo-1550547660-d9450f859349?w=400&h=300&fit=crop", true);
        var burgerBbq = await AddMenuItemAsync(burger.Id, burgerCat1.Id, "BBQ Bacon Smash", 189,
            "Doble smash, tocino ahumado, cebolla crispy y BBQ de la casa.", 15, "https://images.unsplash.com/photo-1553979459-d2229ba9743b?w=400&h=300&fit=crop");
        await AddMenuItemAsync(burger.Id, burgerCat1.Id, "Crispy Chicken", 175,
            "Pollo crujiente, coleslaw, pepinillos y miel de jalapeño.", 16, "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=400&h=300&fit=crop");
        var burgerFries = await AddMenuItemAsync(burger.Id, burgerCat2.Id, "Papas a la francesa", 59,
            "Papas caseras fritas con sal de mar y parmesano.", 8, "https://images.unsplash.com/photo-1573080496219-bb080dd4f877?w=400&h=300&fit=crop");
        await AddMenuItemAsync(burger.Id, burgerCat2.Id, "Aros de cebolla", 69,
            "Aros de cebolla crujientes con salsa de pija.", 10, "https://images.unsplash.com/photo-1639024471283-03518883512d?w=400&h=300&fit=crop");
        var burgerSoda = await AddMenuItemAsync(burger.Id, burgerCat3.Id, "Refresco de cola", 35,
            "Refresco de cola bien frío 600 ml.", 2, "https://images.unsplash.com/photo-1554866585-cd94860890b7?w=400&h=300&fit=crop");
        await AddMenuItemAsync(burger.Id, burgerCat3.Id, "Malteada de vainilla", 69,
            "Malteada cremosa con helado artesanal.", 6, "https://images.unsplash.com/photo-1572490122747-3968b75cc699?w=400&h=300&fit=crop");
        menuItems += 7;

        await _businessHours.ReplaceAsync(burger.Id, CatalogHours(burger.Id, 12, 23));
        var burgerCoupon = new Coupon { Code = "BURGERR10", DiscountType = DiscountType.Percentage, DiscountValue = 10, RestaurantId = burger.Id, ValidFrom = DateTime.UtcNow.AddDays(-10), ValidUntil = DateTime.UtcNow.AddDays(30), MaxUses = 500, MinOrderAmount = 140, IsActive = true };
        await _coupons.AddAsync(burgerCoupon);
        coupons++;

        // === 2. MASA WOK (asiático, Polanco) ===
        var wok = new Restaurant("Masa Wok", "masa-wok-polanco", owner.Id)
        {
            Description = "Wok y comida pan-asiática: tallarines, arroz, dumplings y curries. Recetas rápidas y frescas.",
            Logo = "https://images.unsplash.com/photo-1556742521-9713bf272865?w=200&h=200&fit=crop",
            CoverImage = "https://images.unsplash.com/photo-1540189549336-e6e99c3679fe?w=1200&h=600&fit=crop",
            Phone = "+52 55 8680 4421",
            Address = "Av. Homero 418, Polanco, CDMX",
            Latitude = 19.4306,
            Longitude = -99.1952,
            RadiusKm = 5,
            DeliveryFee = 55,
            MinOrderAmount = 140,
            EstimatedPrepTimeMinutes = 20,
            ThemeConfig = "{\"primaryColor\":\"#c62828\",\"secondaryColor\":\"#4e342e\",\"accentColor\":\"#ffb300\",\"backgroundColor\":\"#fff8e1\",\"textColor\":\"#212121\",\"fontFamily\":\"Inter\"}"
        };
        await _restaurants.AddAsync(wok);
        restaurants++;

        var wokCat1 = new Category("Tallarines", wok.Id) { Description = "Al wok", SortOrder = 1 };
        var wokCat2 = new Category("Dumplings", wok.Id) { Description = "Al vapor o fritos", SortOrder = 2 };
        var wokCat3 = new Category("Bebidas", wok.Id) { Description = "Tés y jugos", SortOrder = 3 };
        await _categories.AddAsync(wokCat1);
        await _categories.AddAsync(wokCat2);
        await _categories.AddAsync(wokCat3);

        var wokNoodles = await AddMenuItemAsync(wok.Id, wokCat1.Id, "Lo Mein de Pollo", 148,
            "Tallarines de huevo salteados al wok con pollo, zanahoria, pimiento y soja.", 15, "https://images.unsplash.com/photo-1512058564366-18510be2db19?w=400&h=300&fit=crop", true);
        await AddMenuItemAsync(wok.Id, wokCat1.Id, "Arroz frito con camarones", 165,
            "Arroz salteado con camarones, arvejas, huevo y cebollín.", 15, "https://images.unsplash.com/photo-1525755662778-989d0524087e?w=400&h=300&fit=crop");
        var wokGyoza = await AddMenuItemAsync(wok.Id, wokCat2.Id, "Gyozas de cerdo", 95,
            "6 gyozas a la plancha con salsa ponzu.", 12, "https://images.unsplash.com/photo-1496116218417-1a781b1c416c?w=400&h=300&fit=crop");
        await AddMenuItemAsync(wok.Id, wokCat2.Id, "Dumplings de camarón", 105,
            "8 dumplings al vapor con salsa de soja.", 14, "https://images.unsplash.com/photo-1541696432-82c6da8ce7bf?w=400&h=300&fit=crop");
        var wokTea = await AddMenuItemAsync(wok.Id, wokCat3.Id, "Té verde helado", 39,
            "Té verde frío con limón y miel.", 3, "https://images.unsplash.com/photo-1556679343-c7306c1976bc?w=400&h=300&fit=crop");
        menuItems += 5;

        await _businessHours.ReplaceAsync(wok.Id, CatalogHours(wok.Id, 12, 23));
        var wokCoupon = new Coupon { Code = "WOK10", DiscountType = DiscountType.Fixed, DiscountValue = 40, RestaurantId = wok.Id, ValidFrom = DateTime.UtcNow.AddDays(-5), ValidUntil = DateTime.UtcNow.AddDays(21), MaxUses = 80, MinOrderAmount = 180, IsActive = true };
        await _coupons.AddAsync(wokCoupon);
        coupons++;

        // === 3. Verde Vida (healthy bowls, Condesa) ===
        var life = new Restaurant("Verde Vida", "verde-vida", owner.Id)
        {
            Description = "Bowls saludables, ensaladas y smoothies con ingredientes orgánicos. Energía real para el día a día.",
            Logo = "https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=200&h=200&fit=crop",
            CoverImage = "https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=1200&h=600&fit=crop",
            Phone = "+52 55 4293 1187",
            Address = "Av. Ámsterdam 230, Hipódromo Condesa, CDMX",
            Latitude = 19.4122,
            Longitude = -99.1707,
            RadiusKm = 4,
            DeliveryFee = 39,
            MinOrderAmount = 100,
            EstimatedPrepTimeMinutes = 15,
            ThemeConfig = "{\"primaryColor\":\"#2e7d32\",\"secondaryColor\":\"#1b5e20\",\"accentColor\":\"#a5d6a7\",\"backgroundColor\":\"#f1f8e9\",\"textColor\":\"#1a1a2e\",\"fontFamily\":\"Inter\"}"
        };
        await _restaurants.AddAsync(life);
        restaurants++;

        var lifeCat1 = new Category("Bowls", life.Id) { Description = "Bowls completos", SortOrder = 1 };
        var lifeCat2 = new Category("Ensaladas", life.Id) { Description = "Mediterránea y completa", SortOrder = 2 };
        var lifeCat3 = new Category("Smoothies", life.Id) { Description = "Frescos, sin azúcar", SortOrder = 3 };
        await _categories.AddAsync(lifeCat1);
        await _categories.AddAsync(lifeCat2);
        await _categories.AddAsync(lifeCat3);

        var lifeQuinoa = await AddMenuItemAsync(life.Id, lifeCat1.Id, "Bowl de quinoa", 129,
            "Quinoa, garbanzos, camote rostizado, aguacate y semillas de girasol.", 12, "https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=400&h=300&fit=crop", true);
        await AddMenuItemAsync(life.Id, lifeCat1.Id, "Bowl de atún poke", 155,
            "Arroz, atún fresco, pepino, edamame y ajonjolí con aderezo de soja.", 14, "https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=400&h=300&fit=crop");
        var lifeCesar = await AddMenuItemAsync(life.Id, lifeCat2.Id, "Ensalada César", 119,
            "Lechuga romana, pollo a la plancha, parmesсов y croutons.", 10, "https://images.unsplash.com/photo-1540420773420-3366772f4999?w=400&h=300&fit=crop");
        await AddMenuItemAsync(life.Id, lifeCat2.Id, "Ensalada griega", 109,
            "Tomate, pepino, falndo, aceite de oliva vetado y olivas.", 8, "https://images.unsplash.com/photo-1540420773420-3366772f4999?w=400&h=300&fit=crop");
        var lifeSmoothie = await AddMenuItemAsync(life.Id, lifeCat3.Id, "Smoothie de mango", 75,
            "Mango, banano y yogurt sin azúcar.", 5, "https://images.unsplash.com/photo-1553530666-ba11a7da3888?w=400&h=300&fit=crop");
        await AddMenuItemAsync(life.Id, lifeCat3.Id, "Smoothie verde", 79,
            "Espinaca, piña, banano y chía.", 5, "https://images.unsplash.com/photo-1553530666-ba11a7da3888?w=400&h=300&fit=crop");
        menuItems += 6;

        await _businessHours.ReplaceAsync(life.Id, CatalogHours(life.Id, 9, 22));
        var lifeCoupon = new Coupon { Code = "LIFE15", DiscountType = DiscountType.Percentage, DiscountValue = 15, RestaurantId = life.Id, ValidFrom = DateTime.UtcNow.AddDays(-7), ValidUntil = DateTime.UtcNow.AddDays(20), MaxUses = 120, MinOrderAmount = 90, IsActive = true };
        await _coupons.AddAsync(lifeCoupon);
        coupons++;

        // === 4. Don Jet (taquería al pastor, La Condesa) ===
        var taqueria = new Restaurant("Don Jet", "don-jet-taqueria", owner.Id)
        {
            Description = "Taquería de barrio con pastor de hoyo, tortillas hechas a mano y salsas de la casa.",
            Logo = "https://images.unsplash.com/photo-1551099810-62f8ba5e6e2b?w=200&h=200&fit=crop",
            CoverImage = "https://images.unsplash.com/photo-1551504734-5ee1c4a1479b?w=1200&h=600&fit=crop",
            Phone = "+52 55 7740 1225",
            Address = "Colima 152, Roma Norte, CDMX",
            Latitude = 19.4200,
            Longitude = -99.1625,
            RadiusKm = 3,
            DeliveryFee = 29,
            MinOrderAmount = 70,
            EstimatedPrepTimeMinutes = 15,
            ThemeConfig = "{\"primaryColor\":\"#c0392b\",\"secondaryColor\":\"#8e44ad\",\"accentColor\":\"#f1c40f\",\"backgroundColor\":\"#fdf6ec\",\"textColor\":\"#1a1a2e\",\"fontFamily\":\"Inter\"}"
        };
        await _restaurants.AddAsync(taqueria);
        restaurants++;

        var taqCat1 = new Category("Tacos", taqueria.Id) { Description = "De la casa", SortOrder = 1 };
        var taqCat2 = new Category("Antojos", taqueria.Id) { Description = "Guarniciones", SortOrder = 2 };
        var taqCat3 = new Category("Bebidas", taqueria.Id) { Description = "Refrescos", SortOrder = 3 };
        await _categories.AddAsync(taqCat1);
        await _categories.AddAsync(taqCat2);
        await _categories.AddAsync(taqCat3);

        var taqPastor = await AddMenuItemAsync(taqueria.Id, taqCat1.Id, "Tacos al pastor (2)", 69,
            "Al pastor de hoyo con piña, cebolla, cilantro, salsa verde y limón.", 6, "https://images.unsplash.com/photo-1551504734-5ee1c4a1479b?w=400&h=300&fit=crop", true);
        await AddMenuItemAsync(taqueria.Id, taqCat1.Id, "Tacos de canasta (5)", 59,
            "Gorditas de chicharrón prensado, papa y frijoles cocidos.", 5, "https://images.unsplash.com/photo-1565299585323-38d6b0865b47?w=400&h=300&fit=crop");
        var taqQuesadilla = await AddMenuItemAsync(taqueria.Id, taqCat2.Id, "Quesadilla de tinga", 55,
            "Tortilla de maíz con tinga de pollo y queso Oaxaca.", 7, "https://images.unsplash.com/photo-1615361200141-f45040f367be?w=400&h=300&fit=crop");
        await AddMenuItemAsync(taqueria.Id, taqCat2.Id, "Frijoles charros", 45,
            "Frijoles de la olla con chorizo.", 8, "https://images.unsplash.com/photo-1626082927389-6cd097cdc6ec?w=400&h=300&fit=crop");
        var taqJamaica = await AddMenuItemAsync(taqueria.Id, taqCat3.Id, "Agua de Jamaica", 25,
            "Agua de jamaica bien fría, 400 ml.", 2, "https://images.unsplash.com/photo-1544145945-f90425340c7e?w=400&h=300&fit=crop");
        menuItems += 5;

        await _businessHours.ReplaceAsync(taqueria.Id, CatalogHours(taqueria.Id, 16, 23));
        var taqCoupon = new Coupon { Code = "TACOS5", DiscountType = DiscountType.Percentage, DiscountValue = 5, RestaurantId = taqueria.Id, ValidFrom = DateTime.UtcNow.AddDays(-2), ValidUntil = DateTime.UtcNow.AddDays(10), MaxUses = 150, MinOrderAmount = 60, IsActive = true };
        await _coupons.AddAsync(taqCoupon);
        coupons++;

        // === 5. MOKA CAFÉ (café de especialidad, La Condesa) ===
        var cafe = new Restaurant("Café Moka", "cafe-moka", owner.Id)
        {
            Description = "Café de especialidad, repostería casera y desayunos todo el día.",
            Logo = "https://images.unsplash.com/photo-1554118811-1e0d58224f24?w=200&h=200&fit=crop",
            CoverImage = "https://images.unsplash.com/photo-1447933601403-0c6688de566e?w=1200&h=600&fit=crop",
            Phone = "+52 55 5564 8821",
            Address = "Av. Michoacán 93, Hipódromo Condesa, CDMX",
            Latitude = 19.4143,
            Longitude = -99.1698,
            RadiusKm = 3,
            DeliveryFee = 35,
            MinOrderAmount = 60,
            EstimatedPrepTimeMinutes = 10,
            ThemeConfig = "{\"primaryColor\":\"#6d4c41\",\"secondaryColor\":\"#3e2723\",\"accentColor\":\"#d7ccc8\",\"backgroundColor\":\"#faf3ea\",\"textColor\":\"#241c15\",\"fontFamily\":\"Inter\"}"
        };
        await _restaurants.AddAsync(cafe);
        restaurants++;

        var cafeCat1 = new Category("Desayunos", cafe.Id) { Description = "Todo el día", SortOrder = 1 };
        var cafeCat2 = new Category("Cafés", cafe.Id) { Description = "De especialidad", SortOrder = 2 };
        var cafeCat3 = new Category("Repostería", cafe.Id) { Description = "Casera", SortOrder = 3 };
        await _categories.AddAsync(cafeCat1);
        await _categories.AddAsync(cafeCat2);
        await _categories.AddAsync(cafeCat3);

        var cafeChilaquiles = await AddMenuItemAsync(cafe.Id, cafeCat1.Id, "Chilaquiles verdes", 99,
            "Chilaquiles con salsa verde, crema, queso fresco y huevo (opcional).", 10, "https://images.unsplash.com/photo-1519214605652-51e9f9d2d5f0?w=400&h=300&fit=crop", true);
        await AddMenuItemAsync(cafe.Id, cafeCat1.Id, "Huevos rancheros", 89,
            "Huevos estrellados sobre tortilla con salsa roja y frijoles.", 10, "https://images.unsplash.com/photo-1547593180-6546ec4cb72f?w=400&h=300&fit=crop");
        var cafeLatte = await AddMenuItemAsync(cafe.Id, cafeCat2.Id, "Latte artesanal", 55,
            "Espresso doble con leche cremada, tamaño 12 oz.", 4, "https://images.unsplash.com/photo-1517701604599-bb29b565090c?w=400&h=300&fit=crop");
        await AddMenuItemAsync(cafe.Id, cafeCat2.Id, "Capuchino", 52,
            "Espresso doble con leche y espuma densa.", 4, "https://images.unsplash.com/photo-1572442388796-11668a67e53d?w=400&h=300&fit=crop");
        var cafeBrownie = await AddMenuItemAsync(cafe.Id, cafeCat3.Id, "Brownie de chocolate", 55,
            "Brownie húmedo con nuez.", 3, "https://images.unsplash.com/photo-1511381939415-e44015466834?w=400&h=300&fit=crop");
        await AddMenuItemAsync(cafe.Id, cafeCat3.Id, "Cinnamon roll", 69,
            "Rollito de canela glaseada.", 5, "https://images.unsplash.com/photo-1511918134-3af4d78b2f55?w=400&h=300&fit=crop");
        menuItems += 6;

        await _businessHours.ReplaceAsync(cafe.Id, CatalogHours(cafe.Id, 8, 19));
        var cafeCoupon = new Coupon { Code = "MOKA10", DiscountType = DiscountType.Percentage, DiscountValue = 10, RestaurantId = cafe.Id, ValidFrom = DateTime.UtcNow.AddDays(-3), ValidUntil = DateTime.UtcNow.AddDays(14), MaxUses = 200, MinOrderAmount = 70, IsActive = true };
        await _coupons.AddAsync(cafeCoupon);
        coupons++;

        // === 6. PIZZERÍA MISS MARGHERITA (napolitana, Coyoacán) ===
        var pizza = new Restaurant("Pizzería Miss Margherita", "miss-margherita", owner.Id)
        {
            Description = "Pizza napolitana con masa 48 horas, horno alto y productos artesanales.",
            Logo = "https://images.unsplash.com/photo-1571407970349-bc81e7e96d47?w=200&h=200&fit=crop",
            CoverImage = "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?w=1200&h=600&fit=crop",
            Phone = "+52 55 5658 1110",
            Address = "Calle Francisco Sosa 99, del Carmen, Coyoacán",
            Latitude = 19.3504,
            Longitude = -99.1688,
            RadiusKm = 5,
            DeliveryFee = 40,
            MinOrderAmount = 120,
            EstimatedPrepTimeMinutes = 30,
            ThemeConfig = "{\"primaryColor\":\"#b71c1c\",\"secondaryColor\":\"#9a4d0e\",\"accentColor\":\"#ffca28\",\"backgroundColor\":\"#fff7ec\",\"textColor\":\"#1a1a2e\",\"fontFamily\":\"Playfair Display\"}"
        };
        await _restaurants.AddAsync(pizza);
        restaurants++;

        var pizzaCat1 = new Category("Pizzas", pizza.Id) { Description = "Napolitanas", SortOrder = 1 };
        var pizzaCat2 = new Category("Entradas", pizza.Id) { Description = "Para compartir", SortOrder = 2 };
        var pizzaCat3 = new Category("Postres", pizza.Id) { Description = "Caseros", SortOrder = 3 };
        await _categories.AddAsync(pizzaCat1);
        await _categories.AddAsync(pizzaCat2);
        await _categories.AddAsync(pizzaCat3);

        var pizzaMargherita = await AddMenuItemAsync(pizza.Id, pizzaCat1.Id, "Pizza Margherita", 179,
            "Tomate San Marzano, mozzarella fior di latte y albahaca fresca.", 15, "https://images.unsplash.com/photo-1574071318508-1cdbab80d002?w=400&h=300&fit=crop", true);
        await AddMenuItemAsync(pizza.Id, pizzaCat1.Id, "Pizza Capricciosa", 205,
            "Jamón, champiñones, alcachofa y aceitunas.", 16, "https://images.unsplash.com/photo-1574071318508-1cdbab80d002?w=400&h=300&fit=crop");
        var pizzaBruschetta = await AddMenuItemAsync(pizza.Id, pizzaCat2.Id, "Bruschetta pomodoro", 75,
            "Pan rústico tostado con tomate, ajo y aceite de oliva.", 8, "https://images.unsplash.com/photo-1555400038-63f5ba517a47?w=400&h=300&fit=crop");
        await AddMenuItemAsync(pizza.Id, pizzaCat2.Id, "Ensalada de rúcula", 65,
            "Rúcula, parmesano en lasca y reducción de balsámico.", 6, "https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=400&h=300&fit=crop");
        await AddMenuItemAsync(pizza.Id, pizzaCat3.Id, "Tiramisú de casa", 95,
            "Capas de mascarpone, café y cacao.", 4, "https://images.unsplash.com/photo-1571877227200-a0d98ea607e9?w=400&h=300&fit=crop");
        menuItems += 5;

        await _businessHours.ReplaceAsync(pizza.Id, CatalogHours(pizza.Id, 13, 23));
        var pizzaCoupon = new Coupon { Code = "MARGH10", DiscountType = DiscountType.Percentage, DiscountValue = 10, RestaurantId = pizza.Id, ValidFrom = DateTime.UtcNow.AddDays(-9), ValidUntil = DateTime.UtcNow.AddDays(18), MaxUses = 200, MinOrderAmount = 130, IsActive = true };
        await _coupons.AddAsync(pizzaCoupon);
        coupons++;

        // === PEDIDOS ENTREGADOS + RESEÑAS (uno por restaurante para que el rating no esté vacío) ===
        var deliveredOrders = new List<(Order Order, Review Review)>
        {
            await SeedCatalogDeliveredOrderAsync(burger, customer, rider, home,
                (burgerSmash, 1), (burgerFries, 1), 4, "Las mejores smash de la Roma. El brioche siempre fresco."),
            await SeedCatalogDeliveredOrderAsync(wok, customer, rider, home,
                (wokNoodles, 1), (wokGyoza, 2), 5, "El Lo Mein llega caliente y las gyozas una delicia."),
            await SeedCatalogDeliveredOrderAsync(life, customer, rider, home,
                (lifeQuinoa, 1), (lifeSmoothie, 1), 4, "Súper fresco, el bowl de quinoa es generoso."),
            await SeedCatalogDeliveredOrderAsync(taqueria, customer, rider, home,
                (taqPastor, 2), (taqJamaica, 1), 5, "El pastor de hoyo se nota en el sabor, llegaron calientes."),
            await SeedCatalogDeliveredOrderAsync(cafe, customer, rider, home,
                (cafeChilaquiles, 1), (cafeLatte, 2), 4, "Los chilaquiles y el latte, lo mejor para empezar el día."),
            await SeedCatalogDeliveredOrderAsync(pizza, customer, rider, home,
                (pizzaMargherita, 1), (pizzaBruschetta, 1), 5, "Masa espectacular, se siente la fermentación larga.")
        };

        orders = deliveredOrders.Count;
        reviews = deliveredOrders.Count;

        return (restaurants, menuItems, coupons, orders, reviews);
    }

    /// <summary>
    /// Crea un pedido entregado completo (historial, pago, reseña) para un restaurante del catálogo.
    /// </summary>
    private async Task<(Order Order, Review Review)> SeedCatalogDeliveredOrderAsync(
        Restaurant restaurant, User customer, Rider rider, string home,
        (MenuItem Item, int Qty) first, (MenuItem Item, int Qty) second,
        int rating, string comment)
    {
        var now = DateTime.UtcNow;
        var created = now.AddMinutes(-now.Minute - Random.Shared.Next(60, 240));
        var subtotal = first.Item.Price * first.Qty + second.Item.Price * second.Qty;
        var total = subtotal + restaurant.DeliveryFee;

        var order = new Order(customer.Id, restaurant.Id)
        {
            Status = OrderStatus.Delivered,
            Total = total,
            DeliveryFee = restaurant.DeliveryFee,
            PaymentStatus = PaymentStatus.Paid,
            DeliveryAddress = home,
            Latitude = 19.4300,
            Longitude = -99.1300,
            RiderId = rider.Id,
            AssignedAt = created.AddMinutes(35),
            PickedUpAt = created.AddMinutes(45),
            DeliveredAt = created.AddMinutes(60),
            CreatedAt = created,
            UpdatedAt = created.AddMinutes(60)
        };
        order.Items.Add(new OrderItem(order.Id, first.Item.Id, first.Qty, first.Item.Price) { CreatedAt = created });
        order.Items.Add(new OrderItem(order.Id, second.Item.Id, second.Qty, second.Item.Price) { CreatedAt = created });

        var steps = new[] { OrderStatus.Pending, OrderStatus.Confirmed, OrderStatus.Preparing, OrderStatus.Ready, OrderStatus.AssignedToRider, OrderStatus.OutForDelivery, OrderStatus.Delivered };
        var minutes = new[] { 5, 15, 28, 35, 45, 60 };
        for (int i = 0; i < minutes.Length; i++)
            order.StatusHistory.Add(new OrderStatusHistory(order.Id, steps[i], steps[i + 1], "demo-seed") { CreatedAt = created.AddMinutes(minutes[i]) });

        order.Payments.Add(new Payment(order.Id, total, "CASH")
        {
            Status = PaymentStatus.Paid,
            TransactionId = $"CASH-{Guid.NewGuid():N}"[..20],
            CreatedAt = created.AddMinutes(3)
        });
        await _orders.AddAsync(order);

        var review = new Review
        {
            RestaurantId = restaurant.Id,
            CustomerId = customer.Id,
            OrderId = order.Id,
            Rating = rating,
            Comment = comment,
            CreatedAt = created.AddMinutes(65)
        };
        await _reviews.AddAsync(review);

        return (order, review);
    }

    private async Task<MenuItem> AddMenuItemAsync(Guid restaurantId, Guid categoryId, string name, decimal price,
        string description, int preparationTime, string image, bool featured = false)
    {
        var item = new MenuItem(name, price, restaurantId, categoryId)
        {
            Description = description,
            PreparationTime = preparationTime,
            Images = new[] { image },
            IsFeatured = featured
        };
        await _menuItems.AddAsync(item);
        return item;
    }

    private static MenuItem MenuItemByName(List<MenuItem> menu, string name) =>
        menu.First(i => i.Name == name);

    private static List<BusinessHour> CatalogHours(Guid restaurantId, int openHour, int closeHour)
    {
        return Enumerable.Range(0, 7)
            .Select(day => new BusinessHour
            {
                RestaurantId = restaurantId,
                DayOfWeek = day,
                OpenTime = new TimeSpan(openHour, 0, 0),
                CloseTime = new TimeSpan(closeHour, 0, 0)
            })
            .ToList();
    }

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
