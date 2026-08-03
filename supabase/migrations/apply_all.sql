-- ============================================================
-- MIGRACIÓN 1: InitialCreate (20260730101034_initial.sql)
-- ============================================================
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

CREATE TABLE "Users" (
    "Id" uuid NOT NULL,
    "Email" character varying(256) NOT NULL,
    "Name" character varying(200) NOT NULL,
    "PasswordHash" text NOT NULL,
    "Role" character varying(50) NOT NULL,
    "Avatar" text NULL,
    "Phone" text NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NULL,
    CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
);

CREATE TABLE "Restaurants" (
    "Id" uuid NOT NULL,
    "OwnerId" uuid NOT NULL,
    "Name" character varying(200) NOT NULL,
    "Slug" character varying(200) NOT NULL,
    "Description" text NULL,
    "Logo" text NULL,
    "CoverImage" text NULL,
    "ThemeConfig" jsonb NULL,
    "IsActive" boolean NOT NULL,
    "Address" text NULL,
    "Phone" text NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NULL,
    CONSTRAINT "PK_Restaurants" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Restaurants_Users_OwnerId" FOREIGN KEY ("OwnerId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "Categories" (
    "Id" uuid NOT NULL,
    "RestaurantId" uuid NOT NULL,
    "Name" character varying(100) NOT NULL,
    "Description" text NULL,
    "Icon" text NULL,
    "SortOrder" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NULL,
    CONSTRAINT "PK_Categories" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Categories_Restaurants_RestaurantId" FOREIGN KEY ("RestaurantId") REFERENCES "Restaurants" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Orders" (
    "Id" uuid NOT NULL,
    "CustomerId" uuid NOT NULL,
    "RestaurantId" uuid NOT NULL,
    "Status" character varying(50) NOT NULL,
    "Total" numeric(18,2) NOT NULL,
    "PaymentStatus" character varying(50) NOT NULL,
    "AiConversationId" uuid NULL,
    "Notes" text NULL,
    "DeliveryAddress" text NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NULL,
    CONSTRAINT "PK_Orders" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Orders_Restaurants_RestaurantId" FOREIGN KEY ("RestaurantId") REFERENCES "Restaurants" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Orders_Users_CustomerId" FOREIGN KEY ("CustomerId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "MenuItems" (
    "Id" uuid NOT NULL,
    "RestaurantId" uuid NOT NULL,
    "CategoryId" uuid NOT NULL,
    "Name" character varying(200) NOT NULL,
    "Description" text NULL,
    "Price" numeric(18,2) NOT NULL,
    "Images" text NOT NULL,
    "IsAvailable" boolean NOT NULL,
    "IsFeatured" boolean NOT NULL,
    "PreparationTime" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NULL,
    CONSTRAINT "PK_MenuItems" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_MenuItems_Categories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES "Categories" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_MenuItems_Restaurants_RestaurantId" FOREIGN KEY ("RestaurantId") REFERENCES "Restaurants" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AIConversations" (
    "Id" uuid NOT NULL,
    "OrderId" uuid NULL,
    "CustomerId" uuid NOT NULL,
    "Messages" jsonb NOT NULL,
    "Summary" text NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NULL,
    CONSTRAINT "PK_AIConversations" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AIConversations_Orders_OrderId" FOREIGN KEY ("OrderId") REFERENCES "Orders" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_AIConversations_Users_CustomerId" FOREIGN KEY ("CustomerId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "OrderStatusHistories" (
    "Id" uuid NOT NULL,
    "OrderId" uuid NOT NULL,
    "FromStatus" character varying(50) NOT NULL,
    "ToStatus" character varying(50) NOT NULL,
    "ChangedBy" character varying(200) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NULL,
    CONSTRAINT "PK_OrderStatusHistories" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_OrderStatusHistories_Orders_OrderId" FOREIGN KEY ("OrderId") REFERENCES "Orders" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Payments" (
    "Id" uuid NOT NULL,
    "OrderId" uuid NOT NULL,
    "Amount" numeric(18,2) NOT NULL,
    "Method" character varying(100) NOT NULL,
    "Status" character varying(50) NOT NULL,
    "TransactionId" text NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NULL,
    CONSTRAINT "PK_Payments" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Payments_Orders_OrderId" FOREIGN KEY ("OrderId") REFERENCES "Orders" ("Id") ON DELETE CASCADE
);

CREATE TABLE "OrderItems" (
    "Id" uuid NOT NULL,
    "OrderId" uuid NOT NULL,
    "MenuItemId" uuid NOT NULL,
    "Quantity" integer NOT NULL,
    "UnitPrice" numeric(18,2) NOT NULL,
    "Notes" text NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NULL,
    CONSTRAINT "PK_OrderItems" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_OrderItems_MenuItems_MenuItemId" FOREIGN KEY ("MenuItemId") REFERENCES "MenuItems" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_OrderItems_Orders_OrderId" FOREIGN KEY ("OrderId") REFERENCES "Orders" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_AIConversations_CustomerId" ON "AIConversations" ("CustomerId");
CREATE UNIQUE INDEX "IX_AIConversations_OrderId" ON "AIConversations" ("OrderId");
CREATE INDEX "IX_Categories_RestaurantId" ON "Categories" ("RestaurantId");
CREATE INDEX "IX_MenuItems_CategoryId" ON "MenuItems" ("CategoryId");
CREATE INDEX "IX_MenuItems_RestaurantId" ON "MenuItems" ("RestaurantId");
CREATE INDEX "IX_OrderItems_MenuItemId" ON "OrderItems" ("MenuItemId");
CREATE INDEX "IX_OrderItems_OrderId" ON "OrderItems" ("OrderId");
CREATE INDEX "IX_Orders_CustomerId" ON "Orders" ("CustomerId");
CREATE INDEX "IX_Orders_RestaurantId" ON "Orders" ("RestaurantId");
CREATE INDEX "IX_OrderStatusHistories_OrderId" ON "OrderStatusHistories" ("OrderId");
CREATE INDEX "IX_Payments_OrderId" ON "Payments" ("OrderId");
CREATE INDEX "IX_Restaurants_OwnerId" ON "Restaurants" ("OwnerId");
CREATE UNIQUE INDEX "IX_Restaurants_Slug" ON "Restaurants" ("Slug");
CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260730101141_InitialCreate', '6.0.0');

COMMIT;

-- ============================================================
-- MIGRACIÓN 2: AddDeliveryAndCommerce (20260801232311_delivery_commerce.sql)
-- ============================================================
START TRANSACTION;

ALTER TABLE "Restaurants" ADD "Latitude" double precision NULL;
ALTER TABLE "Restaurants" ADD "Longitude" double precision NULL;
ALTER TABLE "Restaurants" ADD "RadiusKm" double precision NULL;
ALTER TABLE "Restaurants" ADD "DeliveryFee" numeric(18,2) NOT NULL DEFAULT 0.0;
ALTER TABLE "Restaurants" ADD "MinOrderAmount" numeric(18,2) NOT NULL DEFAULT 0.0;
ALTER TABLE "Restaurants" ADD "EstimatedPrepTimeMinutes" integer NULL;

ALTER TABLE "Orders" ADD "RiderId" uuid NULL;
ALTER TABLE "Orders" ADD "AssignedAt" timestamp with time zone NULL;
ALTER TABLE "Orders" ADD "PickedUpAt" timestamp with time zone NULL;
ALTER TABLE "Orders" ADD "DeliveredAt" timestamp with time zone NULL;
ALTER TABLE "Orders" ADD "Latitude" double precision NULL;
ALTER TABLE "Orders" ADD "Longitude" double precision NULL;

CREATE TABLE "BusinessHours" (
    "Id" uuid NOT NULL,
    "RestaurantId" uuid NOT NULL,
    "DayOfWeek" integer NOT NULL,
    "OpenTime" time NOT NULL,
    "CloseTime" time NOT NULL,
    "IsClosed" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NULL,
    CONSTRAINT "PK_BusinessHours" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_BusinessHours_Restaurants_RestaurantId" FOREIGN KEY ("RestaurantId") REFERENCES "Restaurants" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Coupons" (
    "Id" uuid NOT NULL,
    "Code" character varying(100) NOT NULL,
    "DiscountType" character varying(50) NOT NULL,
    "DiscountValue" numeric(18,2) NOT NULL,
    "RestaurantId" uuid NULL,
    "ValidFrom" timestamp with time zone NOT NULL,
    "ValidUntil" timestamp with time zone NOT NULL,
    "MaxUses" integer NULL,
    "TimesUsed" integer NOT NULL,
    "MinOrderAmount" numeric(18,2) NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NULL,
    CONSTRAINT "PK_Coupons" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Coupons_Restaurants_RestaurantId" FOREIGN KEY ("RestaurantId") REFERENCES "Restaurants" ("Id") ON DELETE SET NULL
);

CREATE TABLE "CustomerAddresses" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Label" character varying(100) NOT NULL,
    "Address" character varying(500) NOT NULL,
    "Latitude" double precision NULL,
    "Longitude" double precision NULL,
    "IsDefault" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NULL,
    CONSTRAINT "PK_CustomerAddresses" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_CustomerAddresses_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Reviews" (
    "Id" uuid NOT NULL,
    "RestaurantId" uuid NOT NULL,
    "CustomerId" uuid NOT NULL,
    "OrderId" uuid NOT NULL,
    "Rating" integer NOT NULL,
    "Comment" character varying(1000) NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NULL,
    CONSTRAINT "PK_Reviews" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Reviews_Orders_OrderId" FOREIGN KEY ("OrderId") REFERENCES "Orders" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Reviews_Restaurants_RestaurantId" FOREIGN KEY ("RestaurantId") REFERENCES "Restaurants" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Reviews_Users_CustomerId" FOREIGN KEY ("CustomerId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Riders" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "VehicleType" character varying(50) NOT NULL,
    "Status" character varying(50) NOT NULL,
    "Latitude" double precision NULL,
    "Longitude" double precision NULL,
    "Rating" numeric(18,2) NOT NULL,
    "RatingsCount" integer NOT NULL,
    "LastLocationAt" timestamp with time zone NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NULL,
    CONSTRAINT "PK_Riders" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Riders_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_Orders_RiderId" ON "Orders" ("RiderId");
CREATE INDEX "IX_BusinessHours_RestaurantId" ON "BusinessHours" ("RestaurantId");
CREATE UNIQUE INDEX "IX_Coupons_Code" ON "Coupons" ("Code");
CREATE INDEX "IX_Coupons_RestaurantId" ON "Coupons" ("RestaurantId");
CREATE INDEX "IX_CustomerAddresses_UserId" ON "CustomerAddresses" ("UserId");
CREATE INDEX "IX_Reviews_CustomerId" ON "Reviews" ("CustomerId");
CREATE UNIQUE INDEX "IX_Reviews_OrderId" ON "Reviews" ("OrderId");
CREATE INDEX "IX_Reviews_RestaurantId" ON "Reviews" ("RestaurantId");
CREATE UNIQUE INDEX "IX_Riders_UserId" ON "Riders" ("UserId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260801232311_AddDeliveryAndCommerce', '8.0.0');

COMMIT;

-- ============================================================
-- MIGRACIÓN 3: AddOrderCouponAndFee (20260801233523_order_coupon_fee.sql)
-- ============================================================
START TRANSACTION;

ALTER TABLE "Payments" ADD "Reference" character varying(200) NULL;

ALTER TABLE "Orders" ADD "CouponId" uuid NULL;
ALTER TABLE "Orders" ADD "DeliveryFee" numeric(18,2) NOT NULL DEFAULT 0.0;
ALTER TABLE "Orders" ADD "DiscountAmount" numeric(18,2) NOT NULL DEFAULT 0.0;

CREATE INDEX "IX_Orders_CouponId" ON "Orders" ("CouponId");

ALTER TABLE "Orders" ADD CONSTRAINT "FK_Orders_Coupons_CouponId"
    FOREIGN KEY ("CouponId") REFERENCES "Coupons" ("Id") ON DELETE SET NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260801233523_AddOrderCouponAndFee', '8.0.0');

COMMIT;

-- ============================================================
-- ROW LEVEL SECURITY (de la migración 2)
-- ============================================================
ALTER TABLE "Riders" ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Riders_select_own" ON "Riders"
    FOR SELECT USING (auth.uid()::text = "UserId"::text);

CREATE POLICY "Riders_insert_own" ON "Riders"
    FOR INSERT WITH CHECK (auth.uid()::text = "UserId"::text);

CREATE POLICY "Riders_update_own" ON "Riders"
    FOR UPDATE USING (auth.uid()::text = "UserId"::text);

ALTER TABLE "CustomerAddresses" ENABLE ROW LEVEL SECURITY;

CREATE POLICY "CustomerAddresses_select_own" ON "CustomerAddresses"
    FOR SELECT USING (auth.uid()::text = "UserId"::text);

CREATE POLICY "CustomerAddresses_insert_own" ON "CustomerAddresses"
    FOR INSERT WITH CHECK (auth.uid()::text = "UserId"::text);

CREATE POLICY "CustomerAddresses_update_own" ON "CustomerAddresses"
    FOR UPDATE USING (auth.uid()::text = "UserId"::text);

CREATE POLICY "CustomerAddresses_delete_own" ON "CustomerAddresses"
    FOR DELETE USING (auth.uid()::text = "UserId"::text);

ALTER TABLE "Reviews" ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Reviews_select_public" ON "Reviews"
    FOR SELECT USING (true);

CREATE POLICY "Reviews_insert_own" ON "Reviews"
    FOR INSERT WITH CHECK (auth.uid()::text = "CustomerId"::text);

ALTER TABLE "Coupons" ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Coupons_select_active_public" ON "Coupons"
    FOR SELECT USING ("IsActive" = true);

ALTER TABLE "BusinessHours" ENABLE ROW LEVEL SECURITY;

CREATE POLICY "BusinessHours_select_public" ON "BusinessHours"
    FOR SELECT USING (true);