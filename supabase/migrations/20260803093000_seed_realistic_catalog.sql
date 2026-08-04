START TRANSACTION;

-- ============================================================================
-- Seed realistic catalog data (idempotent)
-- ----------------------------------------------------------------------------
-- Mirrors SeedController.SeedRealisticCatalogAsync: 6 restaurants, categories,
-- menu items, business hours and coupon per restaurant, plus one delivered
-- order + review each (so ratings never appear empty on a fresh database).
-- Re-runnable: guarded by slug / (restaurant, name) / coupon code / review.
-- Users expected from the base seed: demo@restaurante.app (owner),
-- cliente@restaurante.app (customer), rider@restaurante.app (rider).
-- ============================================================================

-- ---------------------------------------------------------------------------
-- 1. LA BURGER HOUSE (hamburguesas, Roma Norte)
-- ---------------------------------------------------------------------------
DO $$
DECLARE
    r_id    uuid;
    cat_1   uuid;
    cat_2   uuid;
    cat_3   uuid;
    it_1    uuid;
    it_2    uuid;
    c_id    uuid;
    rdr_id  uuid;
    o_id    uuid;
    created_at timestamptz := now() - interval '2 hours';
BEGIN
    -- Restaurant (guard by slug)
    SELECT "Id" INTO r_id FROM "Restaurants" WHERE "Slug" = 'la-burger-house';
    IF r_id IS NULL THEN
        INSERT INTO "Restaurants"
            ("Id", "OwnerId", "Name", "Slug", "Description", "Logo", "CoverImage",
             "ThemeConfig", "IsActive", "Address", "Phone", "Latitude", "Longitude",
             "RadiusKm", "DeliveryFee", "MinOrderAmount", "EstimatedPrepTimeMinutes",
             "CreatedAt", "UpdatedAt")
        VALUES (
            gen_random_uuid(),
            (SELECT "Id" FROM "Users" WHERE "Email" = 'demo@restaurante.app'),
            'La Burger House', 'la-burger-house',
            'Hamburguesas artesanales de res Angus con pan brioche, maduradas y a la leña. Papas caseras y malteadas de temporada.',
            'https://images.unsplash.com/photo-1571091718767-18b5b1457add?w=200&h=200&fit=crop',
            'https://images.unsplash.com/photo-1550547660-d9450f859349?w=1200&h=600&fit=crop',
            '{"primaryColor":"#b2582b","secondaryColor":"#7a3b1c","accentColor":"#f2c14e","backgroundColor":"#fdf6ee","textColor":"#241c15","fontFamily":"Inter"}'::jsonb,
            TRUE, 'Av. Álvaro Obregón 158, Roma Norte, CDMX', '+52 555 214 8890',
            19.4189, -99.1631, 6, 49, 120, 25,
            created_at, created_at)
        RETURNING "Id" INTO r_id;
    END IF;

    -- Categories (guard by restaurant + name)
    SELECT "Id" INTO cat_1 FROM "Categories" WHERE "RestaurantId" = r_id AND "Name" = 'Hamburguesas';
    IF cat_1 IS NULL THEN
        INSERT INTO "Categories" ("Id", "RestaurantId", "Name", "Description", "SortOrder", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, 'Hamburguesas', 'Artesanales', 1, created_at, created_at)
        RETURNING "Id" INTO cat_1;
    END IF;
    SELECT "Id" INTO cat_2 FROM "Categories" WHERE "RestaurantId" = r_id AND "Name" = 'Sides';
    IF cat_2 IS NULL THEN
        INSERT INTO "Categories" ("Id", "RestaurantId", "Name", "Description", "SortOrder", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, 'Sides', 'Guarniciones', 2, created_at, created_at)
        RETURNING "Id" INTO cat_2;
    END IF;
    SELECT "Id" INTO cat_3 FROM "Categories" WHERE "RestaurantId" = r_id AND "Name" = 'Bebidas';
    IF cat_3 IS NULL THEN
        INSERT INTO "Categories" ("Id", "RestaurantId", "Name", "Description", "SortOrder", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, 'Bebidas', 'Refrescos y malteadas', 3, created_at, created_at)
        RETURNING "Id" INTO cat_3;
    END IF;

    -- Menu items (guard by restaurant + name)
    SELECT "Id" INTO it_1 FROM "MenuItems" WHERE "RestaurantId" = r_id AND "Name" = 'Smash Burger Clásica';
    IF it_1 IS NULL THEN
        INSERT INTO "MenuItems" ("Id", "RestaurantId", "CategoryId", "Name", "Description", "Price", "Images", "IsAvailable", "IsFeatured", "PreparationTime", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, cat_1, 'Smash Burger Clásica', 'Doble smash de res, queso americano, cebolla confitada, lechuga y salsa de la casa en pan brioche.', 165, '["https://images.unsplash.com/photo-1550547660-d9450f859349?w=400&h=300&fit=crop"]', TRUE, TRUE, 12, created_at, created_at)
        RETURNING "Id" INTO it_1;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItems" WHERE "RestaurantId" = r_id AND "Name" = 'BBQ Bacon Smash') THEN
        INSERT INTO "MenuItems" ("Id", "RestaurantId", "CategoryId", "Name", "Description", "Price", "Images", "IsAvailable", "IsFeatured", "PreparationTime", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, cat_1, 'BBQ Bacon Smash', 'Doble smash, tocino ahumado, cebolla crispy y BBQ de la casa.', 189, '["https://images.unsplash.com/photo-1553979459-d2229ba9743b?w=400&h=300&fit=crop"]', TRUE, FALSE, 15, created_at, created_at);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItems" WHERE "RestaurantId" = r_id AND "Name" = 'Crispy Chicken') THEN
        INSERT INTO "MenuItems" ("Id", "RestaurantId", "CategoryId", "Name", "Description", "Price", "Images", "IsAvailable", "IsFeatured", "PreparationTime", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, cat_1, 'Crispy Chicken', 'Pollo crujiente, coleslaw, pepinillos y miel de jalapeño.', 175, '["https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=400&h=300&fit=crop"]', TRUE, FALSE, 16, created_at, created_at);
    END IF;
    SELECT "Id" INTO it_2 FROM "MenuItems" WHERE "RestaurantId" = r_id AND "Name" = 'Papas a la francesa';
    IF it_2 IS NULL THEN
        INSERT INTO "MenuItems" ("Id", "RestaurantId", "CategoryId", "Name", "Description", "Price", "Images", "IsAvailable", "IsFeatured", "PreparationTime", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, cat_2, 'Papas a la francesa', 'Papas caseras fritas con sal de mar y parmesano.', 59, '["https://images.unsplash.com/photo-1573080496219-bb080dd4f877?w=400&h=300&fit=crop"]', TRUE, FALSE, 8, created_at, created_at)
        RETURNING "Id" INTO it_2;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItems" WHERE "RestaurantId" = r_id AND "Name" = 'Aros de cebolla') THEN
        INSERT INTO "MenuItems" ("Id", "RestaurantId", "CategoryId", "Name", "Description", "Price", "Images", "IsAvailable", "IsFeatured", "PreparationTime", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, cat_2, 'Aros de cebolla', 'Aros de cebolla crujientes con salsa de la casa.', 69, '["https://images.unsplash.com/photo-1639024471283-03518883512d?w=400&h=300&fit=crop"]', TRUE, FALSE, 10, created_at, created_at);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItems" WHERE "RestaurantId" = r_id AND "Name" = 'Refresco de cola') THEN
        INSERT INTO "MenuItems" ("Id", "RestaurantId", "CategoryId", "Name", "Description", "Price", "Images", "IsAvailable", "IsFeatured", "PreparationTime", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, cat_3, 'Refresco de cola', 'Refresco de cola bien frío 600 ml.', 35, '["https://images.unsplash.com/photo-1554866585-cd94860890b7?w=400&h=300&fit=crop"]', TRUE, FALSE, 2, created_at, created_at);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItems" WHERE "RestaurantId" = r_id AND "Name" = 'Malteada de vainilla') THEN
        INSERT INTO "MenuItems" ("Id", "RestaurantId", "CategoryId", "Name", "Description", "Price", "Images", "IsAvailable", "IsFeatured", "PreparationTime", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, cat_3, 'Malteada de vainilla', 'Malteada cremosa con helado artesanal.', 69, '["https://images.unsplash.com/photo-1572490122747-3968b75cc699?w=400&h=300&fit=crop"]', TRUE, FALSE, 6, created_at, created_at);
    END IF;

    -- Business hours 12:00-23:00 all week (replace)
    DELETE FROM "BusinessHours" WHERE "RestaurantId" = r_id;
    INSERT INTO "BusinessHours" ("Id", "RestaurantId", "DayOfWeek", "OpenTime", "CloseTime", "IsClosed", "CreatedAt", "UpdatedAt")
    SELECT gen_random_uuid(), r_id, d, '12:00'::time, '23:00'::time, FALSE, created_at, created_at
    FROM generate_series(0, 6) AS d;

    -- Coupon (guard by code)
    IF NOT EXISTS (SELECT 1 FROM "Coupons" WHERE "Code" = 'BURGERR10') THEN
        INSERT INTO "Coupons" ("Id", "Code", "DiscountType", "DiscountValue", "RestaurantId", "ValidFrom", "ValidUntil", "MaxUses", "TimesUsed", "MinOrderAmount", "IsActive", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), 'BURGERR10', 'Percentage', 10, r_id, now() - interval '10 days', now() + interval '30 days', 500, 0, 140, TRUE, created_at, created_at);
    END IF;

    -- Delivered order + review (guard: existing review for this restaurant)
    IF NOT EXISTS (SELECT 1 FROM "Reviews" WHERE "RestaurantId" = r_id) THEN
        SELECT "Id" INTO c_id FROM "Users" WHERE "Email" = 'cliente@restaurante.app';
        SELECT r."Id" INTO rdr_id FROM "Riders" r JOIN "Users" u ON u."Id" = r."UserId" WHERE u."Email" = 'rider@restaurante.app';
        IF c_id IS NOT NULL AND rdr_id IS NOT NULL THEN
            INSERT INTO "Orders" ("Id", "CustomerId", "RestaurantId", "Status", "Total", "DeliveryFee", "DiscountAmount", "PaymentStatus", "DeliveryAddress", "Latitude", "Longitude", "RiderId", "AssignedAt", "PickedUpAt", "DeliveredAt", "CreatedAt", "UpdatedAt")
            VALUES (gen_random_uuid(), c_id, r_id, 'Delivered', 273, 49, 0, 'Paid', 'Av. Reforma 123, Ciudad de México', 19.4300, -99.1300, rdr_id, created_at + interval '35 minutes', created_at + interval '45 minutes', created_at + interval '60 minutes', created_at, created_at + interval '60 minutes')
            RETURNING "Id" INTO o_id;

            INSERT INTO "OrderItems" ("Id", "OrderId", "MenuItemId", "Quantity", "UnitPrice", "CreatedAt", "UpdatedAt")
            VALUES (gen_random_uuid(), o_id, it_1, 1, 165, created_at, created_at),
                   (gen_random_uuid(), o_id, it_2, 1, 59, created_at, created_at);

            INSERT INTO "OrderStatusHistories" ("Id", "OrderId", "FromStatus", "ToStatus", "ChangedBy", "CreatedAt", "UpdatedAt") VALUES
                (gen_random_uuid(), o_id, 'Pending', 'Confirmed', 'demo-seed', created_at + interval '5 minutes', NULL),
                (gen_random_uuid(), o_id, 'Confirmed', 'Preparing', 'demo-seed', created_at + interval '15 minutes', NULL),
                (gen_random_uuid(), o_id, 'Preparing', 'Ready', 'demo-seed', created_at + interval '28 minutes', NULL),
                (gen_random_uuid(), o_id, 'Ready', 'AssignedToRider', 'demo-seed', created_at + interval '35 minutes', NULL),
                (gen_random_uuid(), o_id, 'AssignedToRider', 'OutForDelivery', 'demo-seed', created_at + interval '45 minutes', NULL),
                (gen_random_uuid(), o_id, 'OutForDelivery', 'Delivered', 'demo-seed', created_at + interval '60 minutes', NULL);

            INSERT INTO "Payments" ("Id", "OrderId", "Amount", "Method", "Status", "TransactionId", "CreatedAt", "UpdatedAt")
            VALUES (gen_random_uuid(), o_id, 273, 'CASH', 'Paid', 'CASH-BURGER-0001', created_at + interval '3 minutes', NULL);

            INSERT INTO "Reviews" ("Id", "RestaurantId", "CustomerId", "OrderId", "Rating", "Comment", "CreatedAt", "UpdatedAt")
            VALUES (gen_random_uuid(), r_id, c_id, o_id, 4, 'Las mejores smash de la Roma. El brioche siempre fresco.', created_at + interval '65 minutes', NULL);
        END IF;
    END IF;
END $$;

-- ---------------------------------------------------------------------------
-- 2. MASA WOK (pan-asiático, Polanco)
-- ---------------------------------------------------------------------------
DO $$
DECLARE
    r_id    uuid;
    cat_1   uuid;
    cat_2   uuid;
    cat_3   uuid;
    it_1    uuid;
    it_2    uuid;
    c_id    uuid;
    rdr_id  uuid;
    o_id    uuid;
    created_at timestamptz := now() - interval '2 hours';
BEGIN
    SELECT "Id" INTO r_id FROM "Restaurants" WHERE "Slug" = 'masa-wok-polanco';
    IF r_id IS NULL THEN
        INSERT INTO "Restaurants"
            ("Id", "OwnerId", "Name", "Slug", "Description", "Logo", "CoverImage",
             "ThemeConfig", "IsActive", "Address", "Phone", "Latitude", "Longitude",
             "RadiusKm", "DeliveryFee", "MinOrderAmount", "EstimatedPrepTimeMinutes",
             "CreatedAt", "UpdatedAt")
        VALUES (
            gen_random_uuid(),
            (SELECT "Id" FROM "Users" WHERE "Email" = 'demo@restaurante.app'),
            'Masa Wok', 'masa-wok-polanco',
            'Wok y comida pan-asiática: tallarines, arroz, dumplings y curries. Recetas rápidas y frescas.',
            'https://images.unsplash.com/photo-1556742521-9713bf272865?w=200&h=200&fit=crop',
            'https://images.unsplash.com/photo-1540189549336-e6e99c3679fe?w=1200&h=600&fit=crop',
            '{"primaryColor":"#c62828","secondaryColor":"#4e342e","accentColor":"#ffb300","backgroundColor":"#fff8e1","textColor":"#212121","fontFamily":"Inter"}'::jsonb,
            TRUE, 'Av. Homero 418, Polanco, CDMX', '+52 55 8680 4421',
            19.4306, -99.1952, 5, 55, 140, 20,
            created_at, created_at)
        RETURNING "Id" INTO r_id;
    END IF;

    SELECT "Id" INTO cat_1 FROM "Categories" WHERE "RestaurantId" = r_id AND "Name" = 'Tallarines';
    IF cat_1 IS NULL THEN
        INSERT INTO "Categories" ("Id", "RestaurantId", "Name", "Description", "SortOrder", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, 'Tallarines', 'Al wok', 1, created_at, created_at)
        RETURNING "Id" INTO cat_1;
    END IF;
    SELECT "Id" INTO cat_2 FROM "Categories" WHERE "RestaurantId" = r_id AND "Name" = 'Dumplings';
    IF cat_2 IS NULL THEN
        INSERT INTO "Categories" ("Id", "RestaurantId", "Name", "Description", "SortOrder", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, 'Dumplings', 'Al vapor o fritos', 2, created_at, created_at)
        RETURNING "Id" INTO cat_2;
    END IF;
    SELECT "Id" INTO cat_3 FROM "Categories" WHERE "RestaurantId" = r_id AND "Name" = 'Bebidas';
    IF cat_3 IS NULL THEN
        INSERT INTO "Categories" ("Id", "RestaurantId", "Name", "Description", "SortOrder", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, 'Bebidas', 'Tés y jugos', 3, created_at, created_at)
        RETURNING "Id" INTO cat_3;
    END IF;

    SELECT "Id" INTO it_1 FROM "MenuItems" WHERE "RestaurantId" = r_id AND "Name" = 'Lo Mein de Pollo';
    IF it_1 IS NULL THEN
        INSERT INTO "MenuItems" ("Id", "RestaurantId", "CategoryId", "Name", "Description", "Price", "Images", "IsAvailable", "IsFeatured", "PreparationTime", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, cat_1, 'Lo Mein de Pollo', 'Tallarines de huevo salteados al wok con pollo, zanahoria, pimiento y soja.', 148, '["https://images.unsplash.com/photo-1512058564366-18510be2db19?w=400&h=300&fit=crop"]', TRUE, TRUE, 15, created_at, created_at)
        RETURNING "Id" INTO it_1;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItems" WHERE "RestaurantId" = r_id AND "Name" = 'Arroz frito con camarones') THEN
        INSERT INTO "MenuItems" ("Id", "RestaurantId", "CategoryId", "Name", "Description", "Price", "Images", "IsAvailable", "IsFeatured", "PreparationTime", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, cat_1, 'Arroz frito con camarones', 'Arroz salteado con camarones, arvejas, huevo y cebollín.', 165, '["https://images.unsplash.com/photo-1525755662778-989d0524087e?w=400&h=300&fit=crop"]', TRUE, FALSE, 15, created_at, created_at);
    END IF;
    SELECT "Id" INTO it_2 FROM "MenuItems" WHERE "RestaurantId" = r_id AND "Name" = 'Gyozas de cerdo';
    IF it_2 IS NULL THEN
        INSERT INTO "MenuItems" ("Id", "RestaurantId", "CategoryId", "Name", "Description", "Price", "Images", "IsAvailable", "IsFeatured", "PreparationTime", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, cat_2, 'Gyozas de cerdo', '6 gyozas a la plancha con salsa ponzu.', 95, '["https://images.unsplash.com/photo-1496116218417-1a781b1c416c?w=400&h=300&fit=crop"]', TRUE, FALSE, 12, created_at, created_at)
        RETURNING "Id" INTO it_2;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItems" WHERE "RestaurantId" = r_id AND "Name" = 'Dumplings de camarón') THEN
        INSERT INTO "MenuItems" ("Id", "RestaurantId", "CategoryId", "Name", "Description", "Price", "Images", "IsAvailable", "IsFeatured", "PreparationTime", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, cat_2, 'Dumplings de camarón', '8 dumplings al vapor con salsa de soja.', 105, '["https://images.unsplash.com/photo-1541696432-82c6da8ce7bf?w=400&h=300&fit=crop"]', TRUE, FALSE, 14, created_at, created_at);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItems" WHERE "RestaurantId" = r_id AND "Name" = 'Té verde helado') THEN
        INSERT INTO "MenuItems" ("Id", "RestaurantId", "CategoryId", "Name", "Description", "Price", "Images", "IsAvailable", "IsFeatured", "PreparationTime", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, cat_3, 'Té verde helado', 'Té verde frío con limón y miel.', 39, '["https://images.unsplash.com/photo-1556679343-c7306c1976bc?w=400&h=300&fit=crop"]', TRUE, FALSE, 3, created_at, created_at);
    END IF;

    DELETE FROM "BusinessHours" WHERE "RestaurantId" = r_id;
    INSERT INTO "BusinessHours" ("Id", "RestaurantId", "DayOfWeek", "OpenTime", "CloseTime", "IsClosed", "CreatedAt", "UpdatedAt")
    SELECT gen_random_uuid(), r_id, d, '12:00'::time, '23:00'::time, FALSE, created_at, created_at
    FROM generate_series(0, 6) AS d;

    IF NOT EXISTS (SELECT 1 FROM "Coupons" WHERE "Code" = 'WOK10') THEN
        INSERT INTO "Coupons" ("Id", "Code", "DiscountType", "DiscountValue", "RestaurantId", "ValidFrom", "ValidUntil", "MaxUses", "TimesUsed", "MinOrderAmount", "IsActive", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), 'WOK10', 'Fixed', 40, r_id, now() - interval '5 days', now() + interval '21 days', 80, 0, 180, TRUE, created_at, created_at);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM "Reviews" WHERE "RestaurantId" = r_id) THEN
        SELECT "Id" INTO c_id FROM "Users" WHERE "Email" = 'cliente@restaurante.app';
        SELECT r."Id" INTO rdr_id FROM "Riders" r JOIN "Users" u ON u."Id" = r."UserId" WHERE u."Email" = 'rider@restaurante.app';
        IF c_id IS NOT NULL AND rdr_id IS NOT NULL THEN
            INSERT INTO "Orders" ("Id", "CustomerId", "RestaurantId", "Status", "Total", "DeliveryFee", "DiscountAmount", "PaymentStatus", "DeliveryAddress", "Latitude", "Longitude", "RiderId", "AssignedAt", "PickedUpAt", "DeliveredAt", "CreatedAt", "UpdatedAt")
            VALUES (gen_random_uuid(), c_id, r_id, 'Delivered', 393, 55, 0, 'Paid', 'Av. Reforma 123, Ciudad de México', 19.4300, -99.1300, rdr_id, created_at + interval '35 minutes', created_at + interval '45 minutes', created_at + interval '60 minutes', created_at, created_at + interval '60 minutes')
            RETURNING "Id" INTO o_id;

            INSERT INTO "OrderItems" ("Id", "OrderId", "MenuItemId", "Quantity", "UnitPrice", "CreatedAt", "UpdatedAt")
            VALUES (gen_random_uuid(), o_id, it_1, 1, 148, created_at, created_at),
                   (gen_random_uuid(), o_id, it_2, 2, 95, created_at, created_at);

            INSERT INTO "OrderStatusHistories" ("Id", "OrderId", "FromStatus", "ToStatus", "ChangedBy", "CreatedAt", "UpdatedAt") VALUES
                (gen_random_uuid(), o_id, 'Pending', 'Confirmed', 'demo-seed', created_at + interval '5 minutes', NULL),
                (gen_random_uuid(), o_id, 'Confirmed', 'Preparing', 'demo-seed', created_at + interval '15 minutes', NULL),
                (gen_random_uuid(), o_id, 'Preparing', 'Ready', 'demo-seed', created_at + interval '28 minutes', NULL),
                (gen_random_uuid(), o_id, 'Ready', 'AssignedToRider', 'demo-seed', created_at + interval '35 minutes', NULL),
                (gen_random_uuid(), o_id, 'AssignedToRider', 'OutForDelivery', 'demo-seed', created_at + interval '45 minutes', NULL),
                (gen_random_uuid(), o_id, 'OutForDelivery', 'Delivered', 'demo-seed', created_at + interval '60 minutes', NULL);

            INSERT INTO "Payments" ("Id", "OrderId", "Amount", "Method", "Status", "TransactionId", "CreatedAt", "UpdatedAt")
            VALUES (gen_random_uuid(), o_id, 393, 'CASH', 'Paid', 'CASH-WOK-0001', created_at + interval '3 minutes', NULL);

            INSERT INTO "Reviews" ("Id", "RestaurantId", "CustomerId", "OrderId", "Rating", "Comment", "CreatedAt", "UpdatedAt")
            VALUES (gen_random_uuid(), r_id, c_id, o_id, 5, 'El Lo Mein llega caliente y las gyozas una delicia.', created_at + interval '65 minutes', NULL);
        END IF;
    END IF;
END $$;

-- ---------------------------------------------------------------------------
-- 3. VERDE VIDA (bowls saludables, Condesa)
-- ---------------------------------------------------------------------------
DO $$
DECLARE
    r_id    uuid;
    cat_1   uuid;
    cat_2   uuid;
    cat_3   uuid;
    it_1    uuid;
    it_2    uuid;
    c_id    uuid;
    rdr_id  uuid;
    o_id    uuid;
    created_at timestamptz := now() - interval '2 hours';
BEGIN
    SELECT "Id" INTO r_id FROM "Restaurants" WHERE "Slug" = 'verde-vida';
    IF r_id IS NULL THEN
        INSERT INTO "Restaurants"
            ("Id", "OwnerId", "Name", "Slug", "Description", "Logo", "CoverImage",
             "ThemeConfig", "IsActive", "Address", "Phone", "Latitude", "Longitude",
             "RadiusKm", "DeliveryFee", "MinOrderAmount", "EstimatedPrepTimeMinutes",
             "CreatedAt", "UpdatedAt")
        VALUES (
            gen_random_uuid(),
            (SELECT "Id" FROM "Users" WHERE "Email" = 'demo@restaurante.app'),
            'Verde Vida', 'verde-vida',
            'Bowls saludables, ensaladas y smoothies con ingredientes orgánicos. Energía real para el día a día.',
            'https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=200&h=200&fit=crop',
            'https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=1200&h=600&fit=crop',
            '{"primaryColor":"#2e7d32","secondaryColor":"#1b5e20","accentColor":"#a5d6a7","backgroundColor":"#f1f8e9","textColor":"#1a1a2e","fontFamily":"Inter"}'::jsonb,
            TRUE, 'Av. Ámsterdam 230, Hipódromo Condesa, CDMX', '+52 55 4293 1187',
            19.4122, -99.1707, 4, 39, 100, 15,
            created_at, created_at)
        RETURNING "Id" INTO r_id;
    END IF;

    SELECT "Id" INTO cat_1 FROM "Categories" WHERE "RestaurantId" = r_id AND "Name" = 'Bowls';
    IF cat_1 IS NULL THEN
        INSERT INTO "Categories" ("Id", "RestaurantId", "Name", "Description", "SortOrder", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, 'Bowls', 'Bowls completos', 1, created_at, created_at)
        RETURNING "Id" INTO cat_1;
    END IF;
    SELECT "Id" INTO cat_2 FROM "Categories" WHERE "RestaurantId" = r_id AND "Name" = 'Ensaladas';
    IF cat_2 IS NULL THEN
        INSERT INTO "Categories" ("Id", "RestaurantId", "Name", "Description", "SortOrder", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, 'Ensaladas', 'Mediterránea y completa', 2, created_at, created_at)
        RETURNING "Id" INTO cat_2;
    END IF;
    SELECT "Id" INTO cat_3 FROM "Categories" WHERE "RestaurantId" = r_id AND "Name" = 'Smoothies';
    IF cat_3 IS NULL THEN
        INSERT INTO "Categories" ("Id", "RestaurantId", "Name", "Description", "SortOrder", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, 'Smoothies', 'Frescos, sin azúcar', 3, created_at, created_at)
        RETURNING "Id" INTO cat_3;
    END IF;

    SELECT "Id" INTO it_1 FROM "MenuItems" WHERE "RestaurantId" = r_id AND "Name" = 'Bowl de quinoa';
    IF it_1 IS NULL THEN
        INSERT INTO "MenuItems" ("Id", "RestaurantId", "CategoryId", "Name", "Description", "Price", "Images", "IsAvailable", "IsFeatured", "PreparationTime", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, cat_1, 'Bowl de quinoa', 'Quinoa, garbanzos, camote rostizado, aguacate y semillas de girasol.', 129, '["https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=400&h=300&fit=crop"]', TRUE, TRUE, 12, created_at, created_at)
        RETURNING "Id" INTO it_1;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItems" WHERE "RestaurantId" = r_id AND "Name" = 'Bowl de atún poke') THEN
        INSERT INTO "MenuItems" ("Id", "RestaurantId", "CategoryId", "Name", "Description", "Price", "Images", "IsAvailable", "IsFeatured", "PreparationTime", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, cat_1, 'Bowl de atún poke', 'Arroz, atún fresco, pepino, edamame y ajonjolí con aderezo de soja.', 155, '["https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=400&h=300&fit=crop"]', TRUE, FALSE, 14, created_at, created_at);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItems" WHERE "RestaurantId" = r_id AND "Name" = 'Ensalada César') THEN
        INSERT INTO "MenuItems" ("Id", "RestaurantId", "CategoryId", "Name", "Description", "Price", "Images", "IsAvailable", "IsFeatured", "PreparationTime", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, cat_2, 'Ensalada César', 'Lechuga romana, pollo a la plancha, parmesano y croutons.', 119, '["https://images.unsplash.com/photo-1540420773420-3366772f4999?w=400&h=300&fit=crop"]', TRUE, FALSE, 10, created_at, created_at);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItems" WHERE "RestaurantId" = r_id AND "Name" = 'Ensalada griega') THEN
        INSERT INTO "MenuItems" ("Id", "RestaurantId", "CategoryId", "Name", "Description", "Price", "Images", "IsAvailable", "IsFeatured", "PreparationTime", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, cat_2, 'Ensalada griega', 'Tomate, pepino, aceite de oliva y olivas.', 109, '["https://images.unsplash.com/photo-1540420773420-3366772f4999?w=400&h=300&fit=crop"]', TRUE, FALSE, 8, created_at, created_at);
    END IF;
    SELECT "Id" INTO it_2 FROM "MenuItems" WHERE "RestaurantId" = r_id AND "Name" = 'Smoothie de mango';
    IF it_2 IS NULL THEN
        INSERT INTO "MenuItems" ("Id", "RestaurantId", "CategoryId", "Name", "Description", "Price", "Images", "IsAvailable", "IsFeatured", "PreparationTime", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, cat_3, 'Smoothie de mango', 'Mango, banano y yogurt sin azúcar.', 75, '["https://images.unsplash.com/photo-1553530666-ba11a7da3888?w=400&h=300&fit=crop"]', TRUE, FALSE, 5, created_at, created_at)
        RETURNING "Id" INTO it_2;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItems" WHERE "RestaurantId" = r_id AND "Name" = 'Smoothie verde') THEN
        INSERT INTO "MenuItems" ("Id", "RestaurantId", "CategoryId", "Name", "Description", "Price", "Images", "IsAvailable", "IsFeatured", "PreparationTime", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, cat_3, 'Smoothie verde', 'Espinaca, piña, banano y chía.', 79, '["https://images.unsplash.com/photo-1553530666-ba11a7da3888?w=400&h=300&fit=crop"]', TRUE, FALSE, 5, created_at, created_at);
    END IF;

    DELETE FROM "BusinessHours" WHERE "RestaurantId" = r_id;
    INSERT INTO "BusinessHours" ("Id", "RestaurantId", "DayOfWeek", "OpenTime", "CloseTime", "IsClosed", "CreatedAt", "UpdatedAt")
    SELECT gen_random_uuid(), r_id, d, '09:00'::time, '22:00'::time, FALSE, created_at, created_at
    FROM generate_series(0, 6) AS d;

    IF NOT EXISTS (SELECT 1 FROM "Coupons" WHERE "Code" = 'LIFE15') THEN
        INSERT INTO "Coupons" ("Id", "Code", "DiscountType", "DiscountValue", "RestaurantId", "ValidFrom", "ValidUntil", "MaxUses", "TimesUsed", "MinOrderAmount", "IsActive", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), 'LIFE15', 'Percentage', 15, r_id, now() - interval '7 days', now() + interval '20 days', 120, 0, 90, TRUE, created_at, created_at);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM "Reviews" WHERE "RestaurantId" = r_id) THEN
        SELECT "Id" INTO c_id FROM "Users" WHERE "Email" = 'cliente@restaurante.app';
        SELECT r."Id" INTO rdr_id FROM "Riders" r JOIN "Users" u ON u."Id" = r."UserId" WHERE u."Email" = 'rider@restaurante.app';
        IF c_id IS NOT NULL AND rdr_id IS NOT NULL THEN
            INSERT INTO "Orders" ("Id", "CustomerId", "RestaurantId", "Status", "Total", "DeliveryFee", "DiscountAmount", "PaymentStatus", "DeliveryAddress", "Latitude", "Longitude", "RiderId", "AssignedAt", "PickedUpAt", "DeliveredAt", "CreatedAt", "UpdatedAt")
            VALUES (gen_random_uuid(), c_id, r_id, 'Delivered', 243, 39, 0, 'Paid', 'Av. Reforma 123, Ciudad de México', 19.4300, -99.1300, rdr_id, created_at + interval '35 minutes', created_at + interval '45 minutes', created_at + interval '60 minutes', created_at, created_at + interval '60 minutes')
            RETURNING "Id" INTO o_id;

            INSERT INTO "OrderItems" ("Id", "OrderId", "MenuItemId", "Quantity", "UnitPrice", "CreatedAt", "UpdatedAt")
            VALUES (gen_random_uuid(), o_id, it_1, 1, 129, created_at, created_at),
                   (gen_random_uuid(), o_id, it_2, 1, 75, created_at, created_at);

            INSERT INTO "OrderStatusHistories" ("Id", "OrderId", "FromStatus", "ToStatus", "ChangedBy", "CreatedAt", "UpdatedAt") VALUES
                (gen_random_uuid(), o_id, 'Pending', 'Confirmed', 'demo-seed', created_at + interval '5 minutes', NULL),
                (gen_random_uuid(), o_id, 'Confirmed', 'Preparing', 'demo-seed', created_at + interval '15 minutes', NULL),
                (gen_random_uuid(), o_id, 'Preparing', 'Ready', 'demo-seed', created_at + interval '28 minutes', NULL),
                (gen_random_uuid(), o_id, 'Ready', 'AssignedToRider', 'demo-seed', created_at + interval '35 minutes', NULL),
                (gen_random_uuid(), o_id, 'AssignedToRider', 'OutForDelivery', 'demo-seed', created_at + interval '45 minutes', NULL),
                (gen_random_uuid(), o_id, 'OutForDelivery', 'Delivered', 'demo-seed', created_at + interval '60 minutes', NULL);

            INSERT INTO "Payments" ("Id", "OrderId", "Amount", "Method", "Status", "TransactionId", "CreatedAt", "UpdatedAt")
            VALUES (gen_random_uuid(), o_id, 243, 'CASH', 'Paid', 'CASH-LIFE-0001', created_at + interval '3 minutes', NULL);

            INSERT INTO "Reviews" ("Id", "RestaurantId", "CustomerId", "OrderId", "Rating", "Comment", "CreatedAt", "UpdatedAt")
            VALUES (gen_random_uuid(), r_id, c_id, o_id, 4, 'Súper fresco, el bowl de quinoa es generoso.', created_at + interval '65 minutes', NULL);
        END IF;
    END IF;
END $$;

-- ---------------------------------------------------------------------------
-- 4. DON JET (taquería al pastor, Roma Norte)
-- ---------------------------------------------------------------------------
DO $$
DECLARE
    r_id    uuid;
    cat_1   uuid;
    cat_2   uuid;
    cat_3   uuid;
    it_1    uuid;
    it_2    uuid;
    c_id    uuid;
    rdr_id  uuid;
    o_id    uuid;
    created_at timestamptz := now() - interval '2 hours';
BEGIN
    SELECT "Id" INTO r_id FROM "Restaurants" WHERE "Slug" = 'don-jet-taqueria';
    IF r_id IS NULL THEN
        INSERT INTO "Restaurants"
            ("Id", "OwnerId", "Name", "Slug", "Description", "Logo", "CoverImage",
             "ThemeConfig", "IsActive", "Address", "Phone", "Latitude", "Longitude",
             "RadiusKm", "DeliveryFee", "MinOrderAmount", "EstimatedPrepTimeMinutes",
             "CreatedAt", "UpdatedAt")
        VALUES (
            gen_random_uuid(),
            (SELECT "Id" FROM "Users" WHERE "Email" = 'demo@restaurante.app'),
            'Don Jet', 'don-jet-taqueria',
            'Taquería de barrio con pastor de hoyo, tortillas hechas a mano y salsas de la casa.',
            'https://images.unsplash.com/photo-1551099810-62f8ba5e6e2b?w=200&h=200&fit=crop',
            'https://images.unsplash.com/photo-1551504734-5ee1c4a1479b?w=1200&h=600&fit=crop',
            '{"primaryColor":"#c0392b","secondaryColor":"#8e44ad","accentColor":"#f1c40f","backgroundColor":"#fdf6ec","textColor":"#1a1a2e","fontFamily":"Inter"}'::jsonb,
            TRUE, 'Colima 152, Roma Norte, CDMX', '+52 55 7740 1225',
            19.4200, -99.1625, 3, 29, 70, 15,
            created_at, created_at)
        RETURNING "Id" INTO r_id;
    END IF;

    SELECT "Id" INTO cat_1 FROM "Categories" WHERE "RestaurantId" = r_id AND "Name" = 'Tacos';
    IF cat_1 IS NULL THEN
        INSERT INTO "Categories" ("Id", "RestaurantId", "Name", "Description", "SortOrder", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, 'Tacos', 'De la casa', 1, created_at, created_at)
        RETURNING "Id" INTO cat_1;
    END IF;
    SELECT "Id" INTO cat_2 FROM "Categories" WHERE "RestaurantId" = r_id AND "Name" = 'Antojos';
    IF cat_2 IS NULL THEN
        INSERT INTO "Categories" ("Id", "RestaurantId", "Name", "Description", "SortOrder", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, 'Antojos', 'Guarniciones', 2, created_at, created_at)
        RETURNING "Id" INTO cat_2;
    END IF;
    SELECT "Id" INTO cat_3 FROM "Categories" WHERE "RestaurantId" = r_id AND "Name" = 'Bebidas';
    IF cat_3 IS NULL THEN
        INSERT INTO "Categories" ("Id", "RestaurantId", "Name", "Description", "SortOrder", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, 'Bebidas', 'Refrescos', 3, created_at, created_at)
        RETURNING "Id" INTO cat_3;
    END IF;

    SELECT "Id" INTO it_1 FROM "MenuItems" WHERE "RestaurantId" = r_id AND "Name" = 'Tacos al pastor (2)';
    IF it_1 IS NULL THEN
        INSERT INTO "MenuItems" ("Id", "RestaurantId", "CategoryId", "Name", "Description", "Price", "Images", "IsAvailable", "IsFeatured", "PreparationTime", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, cat_1, 'Tacos al pastor (2)', 'Al pastor de hoyo con piña, cebolla, cilantro, salsa verde y limón.', 69, '["https://images.unsplash.com/photo-1551504734-5ee1c4a1479b?w=400&h=300&fit=crop"]', TRUE, TRUE, 6, created_at, created_at)
        RETURNING "Id" INTO it_1;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItems" WHERE "RestaurantId" = r_id AND "Name" = 'Tacos de canasta (5)') THEN
        INSERT INTO "MenuItems" ("Id", "RestaurantId", "CategoryId", "Name", "Description", "Price", "Images", "IsAvailable", "IsFeatured", "PreparationTime", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, cat_1, 'Tacos de canasta (5)', 'Gorditas de chicharrón prensado, papa y frijoles cocidos.', 59, '["https://images.unsplash.com/photo-1565299585323-38d6b0865b47?w=400&h=300&fit=crop"]', TRUE, FALSE, 5, created_at, created_at);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItems" WHERE "RestaurantId" = r_id AND "Name" = 'Quesadilla de tinga') THEN
        INSERT INTO "MenuItems" ("Id", "RestaurantId", "CategoryId", "Name", "Description", "Price", "Images", "IsAvailable", "IsFeatured", "PreparationTime", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, cat_2, 'Quesadilla de tinga', 'Tortilla de maíz con tinga de pollo y queso Oaxaca.', 55, '["https://images.unsplash.com/photo-1615361200141-f45040f367be?w=400&h=300&fit=crop"]', TRUE, FALSE, 7, created_at, created_at);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItems" WHERE "RestaurantId" = r_id AND "Name" = 'Frijoles charros') THEN
        INSERT INTO "MenuItems" ("Id", "RestaurantId", "CategoryId", "Name", "Description", "Price", "Images", "IsAvailable", "IsFeatured", "PreparationTime", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, cat_2, 'Frijoles charros', 'Frijoles de la olla con chorizo.', 45, '["https://images.unsplash.com/photo-1626082927389-6cd097cdc6ec?w=400&h=300&fit=crop"]', TRUE, FALSE, 8, created_at, created_at);
    END IF;
    SELECT "Id" INTO it_2 FROM "MenuItems" WHERE "RestaurantId" = r_id AND "Name" = 'Agua de Jamaica';
    IF it_2 IS NULL THEN
        INSERT INTO "MenuItems" ("Id", "RestaurantId", "CategoryId", "Name", "Description", "Price", "Images", "IsAvailable", "IsFeatured", "PreparationTime", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, cat_3, 'Agua de Jamaica', 'Agua de jamaica bien fría, 400 ml.', 25, '["https://images.unsplash.com/photo-1544145945-f90425340c7e?w=400&h=300&fit=crop"]', TRUE, FALSE, 2, created_at, created_at)
        RETURNING "Id" INTO it_2;
    END IF;

    DELETE FROM "BusinessHours" WHERE "RestaurantId" = r_id;
    INSERT INTO "BusinessHours" ("Id", "RestaurantId", "DayOfWeek", "OpenTime", "CloseTime", "IsClosed", "CreatedAt", "UpdatedAt")
    SELECT gen_random_uuid(), r_id, d, '16:00'::time, '23:00'::time, FALSE, created_at, created_at
    FROM generate_series(0, 6) AS d;

    IF NOT EXISTS (SELECT 1 FROM "Coupons" WHERE "Code" = 'TACOS5') THEN
        INSERT INTO "Coupons" ("Id", "Code", "DiscountType", "DiscountValue", "RestaurantId", "ValidFrom", "ValidUntil", "MaxUses", "TimesUsed", "MinOrderAmount", "IsActive", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), 'TACOS5', 'Percentage', 5, r_id, now() - interval '2 days', now() + interval '10 days', 150, 0, 60, TRUE, created_at, created_at);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM "Reviews" WHERE "RestaurantId" = r_id) THEN
        SELECT "Id" INTO c_id FROM "Users" WHERE "Email" = 'cliente@restaurante.app';
        SELECT r."Id" INTO rdr_id FROM "Riders" r JOIN "Users" u ON u."Id" = r."UserId" WHERE u."Email" = 'rider@restaurante.app';
        IF c_id IS NOT NULL AND rdr_id IS NOT NULL THEN
            INSERT INTO "Orders" ("Id", "CustomerId", "RestaurantId", "Status", "Total", "DeliveryFee", "DiscountAmount", "PaymentStatus", "DeliveryAddress", "Latitude", "Longitude", "RiderId", "AssignedAt", "PickedUpAt", "DeliveredAt", "CreatedAt", "UpdatedAt")
            VALUES (gen_random_uuid(), c_id, r_id, 'Delivered', 192, 29, 0, 'Paid', 'Av. Reforma 123, Ciudad de México', 19.4300, -99.1300, rdr_id, created_at + interval '35 minutes', created_at + interval '45 minutes', created_at + interval '60 minutes', created_at, created_at + interval '60 minutes')
            RETURNING "Id" INTO o_id;

            INSERT INTO "OrderItems" ("Id", "OrderId", "MenuItemId", "Quantity", "UnitPrice", "CreatedAt", "UpdatedAt")
            VALUES (gen_random_uuid(), o_id, it_1, 2, 69, created_at, created_at),
                   (gen_random_uuid(), o_id, it_2, 1, 25, created_at, created_at);

            INSERT INTO "OrderStatusHistories" ("Id", "OrderId", "FromStatus", "ToStatus", "ChangedBy", "CreatedAt", "UpdatedAt") VALUES
                (gen_random_uuid(), o_id, 'Pending', 'Confirmed', 'demo-seed', created_at + interval '5 minutes', NULL),
                (gen_random_uuid(), o_id, 'Confirmed', 'Preparing', 'demo-seed', created_at + interval '15 minutes', NULL),
                (gen_random_uuid(), o_id, 'Preparing', 'Ready', 'demo-seed', created_at + interval '28 minutes', NULL),
                (gen_random_uuid(), o_id, 'Ready', 'AssignedToRider', 'demo-seed', created_at + interval '35 minutes', NULL),
                (gen_random_uuid(), o_id, 'AssignedToRider', 'OutForDelivery', 'demo-seed', created_at + interval '45 minutes', NULL),
                (gen_random_uuid(), o_id, 'OutForDelivery', 'Delivered', 'demo-seed', created_at + interval '60 minutes', NULL);

            INSERT INTO "Payments" ("Id", "OrderId", "Amount", "Method", "Status", "TransactionId", "CreatedAt", "UpdatedAt")
            VALUES (gen_random_uuid(), o_id, 192, 'CASH', 'Paid', 'CASH-JET-0001', created_at + interval '3 minutes', NULL);

            INSERT INTO "Reviews" ("Id", "RestaurantId", "CustomerId", "OrderId", "Rating", "Comment", "CreatedAt", "UpdatedAt")
            VALUES (gen_random_uuid(), r_id, c_id, o_id, 5, 'El pastor de hoyo se nota en el sabor, llegaron calientes.', created_at + interval '65 minutes', NULL);
        END IF;
    END IF;
END $$;

-- ---------------------------------------------------------------------------
-- 5. CAFÉ MOKA (café de especialidad, Condesa)
-- ---------------------------------------------------------------------------
DO $$
DECLARE
    r_id    uuid;
    cat_1   uuid;
    cat_2   uuid;
    cat_3   uuid;
    it_1    uuid;
    it_2    uuid;
    c_id    uuid;
    rdr_id  uuid;
    o_id    uuid;
    created_at timestamptz := now() - interval '2 hours';
BEGIN
    SELECT "Id" INTO r_id FROM "Restaurants" WHERE "Slug" = 'cafe-moka';
    IF r_id IS NULL THEN
        INSERT INTO "Restaurants"
            ("Id", "OwnerId", "Name", "Slug", "Description", "Logo", "CoverImage",
             "ThemeConfig", "IsActive", "Address", "Phone", "Latitude", "Longitude",
             "RadiusKm", "DeliveryFee", "MinOrderAmount", "EstimatedPrepTimeMinutes",
             "CreatedAt", "UpdatedAt")
        VALUES (
            gen_random_uuid(),
            (SELECT "Id" FROM "Users" WHERE "Email" = 'demo@restaurante.app'),
            'Café Moka', 'cafe-moka',
            'Café de especialidad, repostería casera y desayunos todo el día.',
            'https://images.unsplash.com/photo-1554118811-1e0d58224f24?w=200&h=200&fit=crop',
            'https://images.unsplash.com/photo-1447933601403-0c6688de566e?w=1200&h=600&fit=crop',
            '{"primaryColor":"#6d4c41","secondaryColor":"#3e2723","accentColor":"#d7ccc8","backgroundColor":"#faf3ea","textColor":"#241c15","fontFamily":"Inter"}'::jsonb,
            TRUE, 'Av. Michoacán 93, Hipódromo Condesa, CDMX', '+52 55 5564 8821',
            19.4143, -99.1698, 3, 35, 60, 15,
            created_at, created_at)
        RETURNING "Id" INTO r_id;
    END IF;

    SELECT "Id" INTO cat_1 FROM "Categories" WHERE "RestaurantId" = r_id AND "Name" = 'Desayunos';
    IF cat_1 IS NULL THEN
        INSERT INTO "Categories" ("Id", "RestaurantId", "Name", "Description", "SortOrder", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, 'Desayunos', 'Desayunos y brunch', 1, created_at, created_at)
        RETURNING "Id" INTO cat_1;
    END IF;
    SELECT "Id" INTO cat_2 FROM "Categories" WHERE "RestaurantId" = r_id AND "Name" = 'Cafés';
    IF cat_2 IS NULL THEN
        INSERT INTO "Categories" ("Id", "RestaurantId", "Name", "Description", "SortOrder", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, 'Cafés', 'Bebidas calientes', 2, created_at, created_at)
        RETURNING "Id" INTO cat_2;
    END IF;
    SELECT "Id" INTO cat_3 FROM "Categories" WHERE "RestaurantId" = r_id AND "Name" = 'Repostería';
    IF cat_3 IS NULL THEN
        INSERT INTO "Categories" ("Id", "RestaurantId", "Name", "Description", "SortOrder", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, 'Repostería', 'Postres y repostería', 3, created_at, created_at)
        RETURNING "Id" INTO cat_3;
    END IF;

    SELECT "Id" INTO it_1 FROM "MenuItems" WHERE "RestaurantId" = r_id AND "Name" = 'Chilaquiles verdes';
    IF it_1 IS NULL THEN
        INSERT INTO "MenuItems" ("Id", "RestaurantId", "CategoryId", "Name", "Description", "Price", "Images", "IsAvailable", "IsFeatured", "PreparationTime", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, cat_1, 'Chilaquiles verdes', 'Chilaquiles con salsa verde, crema, queso fresco y huevo (opcional).', 99, '["https://images.unsplash.com/photo-1519214605652-51e9f9d2d5f0?w=400&h=300&fit=crop"]', TRUE, TRUE, 10, created_at, created_at)
        RETURNING "Id" INTO it_1;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItems" WHERE "RestaurantId" = r_id AND "Name" = 'Huevos rancheros') THEN
        INSERT INTO "MenuItems" ("Id", "RestaurantId", "CategoryId", "Name", "Description", "Price", "Images", "IsAvailable", "IsFeatured", "PreparationTime", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, cat_1, 'Huevos rancheros', 'Huevos estrellados sobre tortilla con salsa roja y frijoles.', 89, '["https://images.unsplash.com/photo-1547593180-6546ec4cb72f?w=400&h=300&fit=crop"]', TRUE, FALSE, 10, created_at, created_at);
    END IF;
    SELECT "Id" INTO it_2 FROM "MenuItems" WHERE "RestaurantId" = r_id AND "Name" = 'Latte artesanal';
    IF it_2 IS NULL THEN
        INSERT INTO "MenuItems" ("Id", "RestaurantId", "CategoryId", "Name", "Description", "Price", "Images", "IsAvailable", "IsFeatured", "PreparationTime", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, cat_2, 'Latte artesanal', 'Espresso doble con leche cremada, tamaño 12 oz.', 55, '["https://images.unsplash.com/photo-1517701604599-bb29b565090c?w=400&h=300&fit=crop"]', TRUE, FALSE, 4, created_at, created_at)
        RETURNING "Id" INTO it_2;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItems" WHERE "RestaurantId" = r_id AND "Name" = 'Capuchino') THEN
        INSERT INTO "MenuItems" ("Id", "RestaurantId", "CategoryId", "Name", "Description", "Price", "Images", "IsAvailable", "IsFeatured", "PreparationTime", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, cat_2, 'Capuchino', 'Espresso doble con leche y espuma densa.', 52, '["https://images.unsplash.com/photo-1572442388796-11668a67e53d?w=400&h=300&fit=crop"]', TRUE, FALSE, 4, created_at, created_at);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItems" WHERE "RestaurantId" = r_id AND "Name" = 'Brownie de chocolate') THEN
        INSERT INTO "MenuItems" ("Id", "RestaurantId", "CategoryId", "Name", "Description", "Price", "Images", "IsAvailable", "IsFeatured", "PreparationTime", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, cat_3, 'Brownie de chocolate', 'Brownie húmedo con nuez.', 55, '["https://images.unsplash.com/photo-1511381939415-e44015466834?w=400&h=300&fit=crop"]', TRUE, FALSE, 3, created_at, created_at);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItems" WHERE "RestaurantId" = r_id AND "Name" = 'Cinnamon roll') THEN
        INSERT INTO "MenuItems" ("Id", "RestaurantId", "CategoryId", "Name", "Description", "Price", "Images", "IsAvailable", "IsFeatured", "PreparationTime", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, cat_3, 'Cinnamon roll', 'Rollito de canela glaseado.', 69, '["https://images.unsplash.com/photo-1511918134-3af4d78b2f55?w=400&h=300&fit=crop"]', TRUE, FALSE, 5, created_at, created_at);
    END IF;

    DELETE FROM "BusinessHours" WHERE "RestaurantId" = r_id;
    INSERT INTO "BusinessHours" ("Id", "RestaurantId", "DayOfWeek", "OpenTime", "CloseTime", "IsClosed", "CreatedAt", "UpdatedAt")
    SELECT gen_random_uuid(), r_id, d, '08:00'::time, '19:00'::time, FALSE, created_at, created_at
    FROM generate_series(0, 6) AS d;

    IF NOT EXISTS (SELECT 1 FROM "Coupons" WHERE "Code" = 'MOKA10') THEN
        INSERT INTO "Coupons" ("Id", "Code", "DiscountType", "DiscountValue", "RestaurantId", "ValidFrom", "ValidUntil", "MaxUses", "TimesUsed", "MinOrderAmount", "IsActive", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), 'MOKA10', 'Percentage', 10, r_id, now() - interval '3 days', now() + interval '14 days', 200, 0, 70, TRUE, created_at, created_at);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM "Reviews" WHERE "RestaurantId" = r_id) THEN
        SELECT "Id" INTO c_id FROM "Users" WHERE "Email" = 'cliente@restaurante.app';
        SELECT r."Id" INTO rdr_id FROM "Riders" r JOIN "Users" u ON u."Id" = r."UserId" WHERE u."Email" = 'rider@restaurante.app';
        IF c_id IS NOT NULL AND rdr_id IS NOT NULL THEN
            INSERT INTO "Orders" ("Id", "CustomerId", "RestaurantId", "Status", "Total", "DeliveryFee", "DiscountAmount", "PaymentStatus", "DeliveryAddress", "Latitude", "Longitude", "RiderId", "AssignedAt", "PickedUpAt", "DeliveredAt", "CreatedAt", "UpdatedAt")
            VALUES (gen_random_uuid(), c_id, r_id, 'Delivered', 244, 35, 0, 'Paid', 'Av. Reforma 123, Ciudad de México', 19.4300, -99.1300, rdr_id, created_at + interval '35 minutes', created_at + interval '45 minutes', created_at + interval '60 minutes', created_at, created_at + interval '60 minutes')
            RETURNING "Id" INTO o_id;

            INSERT INTO "OrderItems" ("Id", "OrderId", "MenuItemId", "Quantity", "UnitPrice", "CreatedAt", "UpdatedAt")
            VALUES (gen_random_uuid(), o_id, it_1, 1, 99, created_at, created_at),
                   (gen_random_uuid(), o_id, it_2, 2, 55, created_at, created_at);

            INSERT INTO "OrderStatusHistories" ("Id", "OrderId", "FromStatus", "ToStatus", "ChangedBy", "CreatedAt", "UpdatedAt") VALUES
                (gen_random_uuid(), o_id, 'Pending', 'Confirmed', 'demo-seed', created_at + interval '5 minutes', NULL),
                (gen_random_uuid(), o_id, 'Confirmed', 'Preparing', 'demo-seed', created_at + interval '15 minutes', NULL),
                (gen_random_uuid(), o_id, 'Preparing', 'Ready', 'demo-seed', created_at + interval '28 minutes', NULL),
                (gen_random_uuid(), o_id, 'Ready', 'AssignedToRider', 'demo-seed', created_at + interval '35 minutes', NULL),
                (gen_random_uuid(), o_id, 'AssignedToRider', 'OutForDelivery', 'demo-seed', created_at + interval '45 minutes', NULL),
                (gen_random_uuid(), o_id, 'OutForDelivery', 'Delivered', 'demo-seed', created_at + interval '60 minutes', NULL);

            INSERT INTO "Payments" ("Id", "OrderId", "Amount", "Method", "Status", "TransactionId", "CreatedAt", "UpdatedAt")
            VALUES (gen_random_uuid(), o_id, 244, 'CASH', 'Paid', 'CASH-MOKA-0001', created_at + interval '3 minutes', NULL);

            INSERT INTO "Reviews" ("Id", "RestaurantId", "CustomerId", "OrderId", "Rating", "Comment", "CreatedAt", "UpdatedAt")
            VALUES (gen_random_uuid(), r_id, c_id, o_id, 4, 'Los chilaquiles y el latte, lo mejor para empezar el día.', created_at + interval '65 minutes', NULL);
        END IF;
    END IF;
END $$;

-- ---------------------------------------------------------------------------
-- 6. PIZZERÍA MISS MARGHERITA (napolitana, Coyoacán)
-- ---------------------------------------------------------------------------
DO $$
DECLARE
    r_id    uuid;
    cat_1   uuid;
    cat_2   uuid;
    cat_3   uuid;
    it_1    uuid;
    it_2    uuid;
    c_id    uuid;
    rdr_id  uuid;
    o_id    uuid;
    created_at timestamptz := now() - interval '2 hours';
BEGIN
    SELECT "Id" INTO r_id FROM "Restaurants" WHERE "Slug" = 'miss-margherita';
    IF r_id IS NULL THEN
        INSERT INTO "Restaurants"
            ("Id", "OwnerId", "Name", "Slug", "Description", "Logo", "CoverImage",
             "ThemeConfig", "IsActive", "Address", "Phone", "Latitude", "Longitude",
             "RadiusKm", "DeliveryFee", "MinOrderAmount", "EstimatedPrepTimeMinutes",
             "CreatedAt", "UpdatedAt")
        VALUES (
            gen_random_uuid(),
            (SELECT "Id" FROM "Users" WHERE "Email" = 'demo@restaurante.app'),
            'Pizzería Miss Margherita', 'miss-margherita',
            'Pizza napolitana con masa 48 horas, horno alto y productos artesanales.',
            'https://images.unsplash.com/photo-1571407970349-bc81e7e96d47?w=200&h=200&fit=crop',
            'https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?w=1200&h=600&fit=crop',
            '{"primaryColor":"#b71c1c","secondaryColor":"#9a4d0e","accentColor":"#ffca28","backgroundColor":"#fff7ec","textColor":"#1a1a2e","fontFamily":"Playfair Display"}'::jsonb,
            TRUE, 'Calle Francisco Sosa 99, del Carmen, Coyoacán', '+52 55 5658 1110',
            19.3504, -99.1688, 5, 40, 120, 30,
            created_at, created_at)
        RETURNING "Id" INTO r_id;
    END IF;

    SELECT "Id" INTO cat_1 FROM "Categories" WHERE "RestaurantId" = r_id AND "Name" = 'Pizzas';
    IF cat_1 IS NULL THEN
        INSERT INTO "Categories" ("Id", "RestaurantId", "Name", "Description", "SortOrder", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, 'Pizzas', 'Napolitanas', 1, created_at, created_at)
        RETURNING "Id" INTO cat_1;
    END IF;
    SELECT "Id" INTO cat_2 FROM "Categories" WHERE "RestaurantId" = r_id AND "Name" = 'Entradas';
    IF cat_2 IS NULL THEN
        INSERT INTO "Categories" ("Id", "RestaurantId", "Name", "Description", "SortOrder", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, 'Entradas', 'Para compartir', 2, created_at, created_at)
        RETURNING "Id" INTO cat_2;
    END IF;
    SELECT "Id" INTO cat_3 FROM "Categories" WHERE "RestaurantId" = r_id AND "Name" = 'Postres';
    IF cat_3 IS NULL THEN
        INSERT INTO "Categories" ("Id", "RestaurantId", "Name", "Description", "SortOrder", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, 'Postres', 'Caseros', 3, created_at, created_at)
        RETURNING "Id" INTO cat_3;
    END IF;

    SELECT "Id" INTO it_1 FROM "MenuItems" WHERE "RestaurantId" = r_id AND "Name" = 'Pizza Margherita';
    IF it_1 IS NULL THEN
        INSERT INTO "MenuItems" ("Id", "RestaurantId", "CategoryId", "Name", "Description", "Price", "Images", "IsAvailable", "IsFeatured", "PreparationTime", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, cat_1, 'Pizza Margherita', 'Tomate San Marzano, mozzarella fior di latte y albahaca fresca.', 179, '["https://images.unsplash.com/photo-1574071318508-1cdbab80d002?w=400&h=300&fit=crop"]', TRUE, TRUE, 15, created_at, created_at)
        RETURNING "Id" INTO it_1;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItems" WHERE "RestaurantId" = r_id AND "Name" = 'Pizza Capricciosa') THEN
        INSERT INTO "MenuItems" ("Id", "RestaurantId", "CategoryId", "Name", "Description", "Price", "Images", "IsAvailable", "IsFeatured", "PreparationTime", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, cat_1, 'Pizza Capricciosa', 'Jamón, champiñones, alcachofa y aceitunas.', 205, '["https://images.unsplash.com/photo-1574071318508-1cdbab80d002?w=400&h=300&fit=crop"]', TRUE, FALSE, 16, created_at, created_at);
    END IF;
    SELECT "Id" INTO it_2 FROM "MenuItems" WHERE "RestaurantId" = r_id AND "Name" = 'Bruschetta pomodoro';
    IF it_2 IS NULL THEN
        INSERT INTO "MenuItems" ("Id", "RestaurantId", "CategoryId", "Name", "Description", "Price", "Images", "IsAvailable", "IsFeatured", "PreparationTime", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, cat_2, 'Bruschetta pomodoro', 'Pan rústico tostado con tomate, ajo y aceite de oliva.', 75, '["https://images.unsplash.com/photo-1555400038-63f5ba517a47?w=400&h=300&fit=crop"]', TRUE, FALSE, 8, created_at, created_at)
        RETURNING "Id" INTO it_2;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItems" WHERE "RestaurantId" = r_id AND "Name" = 'Ensalada de rúcula') THEN
        INSERT INTO "MenuItems" ("Id", "RestaurantId", "CategoryId", "Name", "Description", "Price", "Images", "IsAvailable", "IsFeatured", "PreparationTime", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, cat_2, 'Ensalada de rúcula', 'Rúcula, parmesano en lasca y reducción de balsámico.', 65, '["https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=400&h=300&fit=crop"]', TRUE, FALSE, 6, created_at, created_at);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItems" WHERE "RestaurantId" = r_id AND "Name" = 'Tiramisú de casa') THEN
        INSERT INTO "MenuItems" ("Id", "RestaurantId", "CategoryId", "Name", "Description", "Price", "Images", "IsAvailable", "IsFeatured", "PreparationTime", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), r_id, cat_3, 'Tiramisú de casa', 'Capas de mascarpone, café y cacao.', 95, '["https://images.unsplash.com/photo-1571877227200-a0d98ea607e9?w=400&h=300&fit=crop"]', TRUE, FALSE, 4, created_at, created_at);
    END IF;

    DELETE FROM "BusinessHours" WHERE "RestaurantId" = r_id;
    INSERT INTO "BusinessHours" ("Id", "RestaurantId", "DayOfWeek", "OpenTime", "CloseTime", "IsClosed", "CreatedAt", "UpdatedAt")
    SELECT gen_random_uuid(), r_id, d, '13:00'::time, '23:00'::time, FALSE, created_at, created_at
    FROM generate_series(0, 6) AS d;

    IF NOT EXISTS (SELECT 1 FROM "Coupons" WHERE "Code" = 'MARGH10') THEN
        INSERT INTO "Coupons" ("Id", "Code", "DiscountType", "DiscountValue", "RestaurantId", "ValidFrom", "ValidUntil", "MaxUses", "TimesUsed", "MinOrderAmount", "IsActive", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), 'MARGH10', 'Percentage', 10, r_id, now() - interval '9 days', now() + interval '18 days', 200, 0, 130, TRUE, created_at, created_at);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM "Reviews" WHERE "RestaurantId" = r_id) THEN
        SELECT "Id" INTO c_id FROM "Users" WHERE "Email" = 'cliente@restaurante.app';
        SELECT r."Id" INTO rdr_id FROM "Riders" r JOIN "Users" u ON u."Id" = r."UserId" WHERE u."Email" = 'rider@restaurante.app';
        IF c_id IS NOT NULL AND rdr_id IS NOT NULL THEN
            INSERT INTO "Orders" ("Id", "CustomerId", "RestaurantId", "Status", "Total", "DeliveryFee", "DiscountAmount", "PaymentStatus", "DeliveryAddress", "Latitude", "Longitude", "RiderId", "AssignedAt", "PickedUpAt", "DeliveredAt", "CreatedAt", "UpdatedAt")
            VALUES (gen_random_uuid(), c_id, r_id, 'Delivered', 294, 40, 0, 'Paid', 'Av. Reforma 123, Ciudad de México', 19.4300, -99.1300, rdr_id, created_at + interval '35 minutes', created_at + interval '45 minutes', created_at + interval '60 minutes', created_at, created_at + interval '60 minutes')
            RETURNING "Id" INTO o_id;

            INSERT INTO "OrderItems" ("Id", "OrderId", "MenuItemId", "Quantity", "UnitPrice", "CreatedAt", "UpdatedAt")
            VALUES (gen_random_uuid(), o_id, it_1, 1, 179, created_at, created_at),
                   (gen_random_uuid(), o_id, it_2, 1, 75, created_at, created_at);

            INSERT INTO "OrderStatusHistories" ("Id", "OrderId", "FromStatus", "ToStatus", "ChangedBy", "CreatedAt", "UpdatedAt") VALUES
                (gen_random_uuid(), o_id, 'Pending', 'Confirmed', 'demo-seed', created_at + interval '5 minutes', NULL),
                (gen_random_uuid(), o_id, 'Confirmed', 'Preparing', 'demo-seed', created_at + interval '15 minutes', NULL),
                (gen_random_uuid(), o_id, 'Preparing', 'Ready', 'demo-seed', created_at + interval '28 minutes', NULL),
                (gen_random_uuid(), o_id, 'Ready', 'AssignedToRider', 'demo-seed', created_at + interval '35 minutes', NULL),
                (gen_random_uuid(), o_id, 'AssignedToRider', 'OutForDelivery', 'demo-seed', created_at + interval '45 minutes', NULL),
                (gen_random_uuid(), o_id, 'OutForDelivery', 'Delivered', 'demo-seed', created_at + interval '60 minutes', NULL);

            INSERT INTO "Payments" ("Id", "OrderId", "Amount", "Method", "Status", "TransactionId", "CreatedAt", "UpdatedAt")
            VALUES (gen_random_uuid(), o_id, 294, 'CASH', 'Paid', 'CASH-PIZZA-0001', created_at + interval '3 minutes', NULL);

            INSERT INTO "Reviews" ("Id", "RestaurantId", "CustomerId", "OrderId", "Rating", "Comment", "CreatedAt", "UpdatedAt")
            VALUES (gen_random_uuid(), r_id, c_id, o_id, 5, 'Masa espectacular, se siente la fermentación larga.', created_at + interval '65 minutes', NULL);
        END IF;
    END IF;
END $$;

-- ---------------------------------------------------------------------------
-- Register migration in EF history
-- ---------------------------------------------------------------------------
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260803093000_SeedRealisticCatalog', '8.0.0');

COMMIT;
