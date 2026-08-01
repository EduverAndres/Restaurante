START TRANSACTION;

-- Restaurants: delivery/commerce columns
ALTER TABLE "Restaurants" ADD "Latitude" double precision NULL;
ALTER TABLE "Restaurants" ADD "Longitude" double precision NULL;
ALTER TABLE "Restaurants" ADD "RadiusKm" double precision NULL;
ALTER TABLE "Restaurants" ADD "DeliveryFee" numeric(18,2) NOT NULL DEFAULT 0.0;
ALTER TABLE "Restaurants" ADD "MinOrderAmount" numeric(18,2) NOT NULL DEFAULT 0.0;
ALTER TABLE "Restaurants" ADD "EstimatedPrepTimeMinutes" integer NULL;

-- Orders: delivery tracking columns
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

-- ---------------------------------------------------------------------------
-- Row Level Security
-- ---------------------------------------------------------------------------
-- The .NET API connects with the service role (BYPASSRLS), so the policies
-- below do NOT affect the application: the API owns all writes. RLS exists so
-- direct client access (e.g. a future mobile app using Supabase Auth) is safe
-- by default: users only reach their own rows, while reviews, coupons and
-- business hours are public-readable. Management (insert/update) of reviews,
-- coupons and business hours is intentionally API-only.

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
