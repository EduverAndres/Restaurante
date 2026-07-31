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
    private readonly IPasswordService _password;

    public SeedController(
        IUserRepository users, IRestaurantRepository restaurants,
        ICategoryRepository categories, IMenuItemRepository menuItems,
        IPasswordService password)
    {
        _users = users;
        _restaurants = restaurants;
        _categories = categories;
        _menuItems = menuItems;
        _password = password;
    }

    [HttpPost("seed")]
    public async Task<IActionResult> Seed()
    {
        var existing = await _users.GetByEmailAsync("demo@restaurante.app");
        if (existing != null)
            return Ok(new { message = "Seed data already exists", demoEmail = "demo@restaurante.app", demoPassword = "Demo123!" });

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
            ThemeConfig = "{\"primaryColor\":\"#d4852a\",\"secondaryColor\":\"#8c4f1d\",\"accentColor\":\"#e8bb7d\",\"backgroundColor\":\"#fdf8f0\",\"textColor\":\"#1a1a2e\",\"fontFamily\":\"Inter\"}"
        };
        await _restaurants.AddAsync(taco);

        var tacoCat1 = new Category("Tacos", taco.Id) { Description = "Tacos tradicionales", SortOrder = 1 };
        var tacoCat2 = new Category("Quesadillas", taco.Id) { Description = "Quesadillas doraditas", SortOrder = 2 };
        var tacoCat3 = new Category("Bebidas", taco.Id) { Description = "Refrescantes", SortOrder = 3 };
        await _categories.AddAsync(tacoCat1);
        await _categories.AddAsync(tacoCat2);
        await _categories.AddAsync(tacoCat3);

        await _menuItems.AddAsync(new MenuItem("Tacos al Pastor", 89, taco.Id, tacoCat1.Id) { Description = "Tortilla de maíz con carne al pastor, piña, cebolla y cilantro", PreparationTime = 10, Images = new string[] { "https://images.unsplash.com/photo-1551504734-5ee1c4a1479b?w=400&h=300&fit=crop" }, IsFeatured = true });
        await _menuItems.AddAsync(new MenuItem("Tacos de Carnitas", 99, taco.Id, tacoCat1.Id) { Description = "Tortilla de maíz con carnitas de cerdo, salsa verde y aguacate", PreparationTime = 12, Images = new string[] { "https://images.unsplash.com/photo-1551504734-5ee1c4a1479b?w=400&h=300&fit=crop" } });
        await _menuItems.AddAsync(new MenuItem("Quesadilla de Huitlacoche", 79, taco.Id, tacoCat2.Id) { Description = "Quesadilla de maíz azul rellena de huitlacoche y queso Oaxaca", PreparationTime = 8, Images = new string[] { "https://images.unsplash.com/photo-1615361200141-f45040f367be?w=400&h=300&fit=crop" }, IsFeatured = true });
        await _menuItems.AddAsync(new MenuItem("Quesadilla de Champiñones", 69, taco.Id, tacoCat2.Id) { Description = "Quesadilla de harina con champiñones salteados y queso manchego", PreparationTime = 8, Images = new string[] { "https://images.unsplash.com/photo-1615361200141-f45040f367be?w=400&h=300&fit=crop" } });
        await _menuItems.AddAsync(new MenuItem("Agua de Horchata", 35, taco.Id, tacoCat3.Id) { Description = "Agua fresca de horchata con canela", PreparationTime = 2, Images = new string[] { "https://images.unsplash.com/photo-1544145945-f90425340c7e?w=400&h=300&fit=crop" } });
        await _menuItems.AddAsync(new MenuItem("Jamaica", 30, taco.Id, tacoCat3.Id) { Description = "Agua de jamaica bien fría", PreparationTime = 2, Images = new string[] { "https://images.unsplash.com/photo-1544145945-f90425340c7e?w=400&h=300&fit=crop" } });

        // === 2. DA VINCI'S TABLE ===
        var italian = new Restaurant("Da Vinci's Table", "da-vincis-table", owner.Id)
        {
            Description = "Cocina italiana artesanal. Pastas frescas, pizzas al horno de leña y postres tradicionales.",
            Logo = "https://images.unsplash.com/photo-1574071318508-1cdbab80d002?w=200&h=200&fit=crop",
            CoverImage = "https://images.unsplash.com/photo-1555396273-367ea4eb4db5?w=1200&h=600&fit=crop",
            Phone = "+52 555 987 6543",
            Address = "Callejón del Artista 45, Roma Norte",
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

        return Ok(new
        {
            message = "Seed data created successfully",
            demoEmail = "demo@restaurante.app",
            customerEmail = "cliente@restaurante.app",
            demoPassword = "Demo123!",
            restaurantsCreated = 3,
            usersCreated = 2
        });
    }
}
