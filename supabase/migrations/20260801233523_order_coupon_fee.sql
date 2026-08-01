START TRANSACTION;

-- Payments: Wompi reference column
ALTER TABLE "Payments" ADD "Reference" character varying(200) NULL;

-- Orders: coupon + fee columns
ALTER TABLE "Orders" ADD "CouponId" uuid NULL;
ALTER TABLE "Orders" ADD "DeliveryFee" numeric(18,2) NOT NULL DEFAULT 0.0;
ALTER TABLE "Orders" ADD "DiscountAmount" numeric(18,2) NOT NULL DEFAULT 0.0;

CREATE INDEX "IX_Orders_CouponId" ON "Orders" ("CouponId");

ALTER TABLE "Orders" ADD CONSTRAINT "FK_Orders_Coupons_CouponId"
    FOREIGN KEY ("CouponId") REFERENCES "Coupons" ("Id") ON DELETE SET NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260801233523_AddOrderCouponAndFee', '8.0.0');

COMMIT;
