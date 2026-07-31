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

