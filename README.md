# Restaurante — Food Delivery Platform

Full-stack food-delivery platform ("demo delivery app"): customers browse restaurants, order with
live AI chat assistance, track riders on a map in real time, and pay by card (Wompi) or cash on
delivery; restaurant owners manage menus, business hours, delivery zones, coupons and live orders;
riders stream their location during deliveries.

## Stack

| Layer    | Technology |
|----------|-----------|
| Backend  | ASP.NET Core 8, Clean Architecture + CQRS (MediatR 11), EF Core 8 |
| Database | SQLite (development) / PostgreSQL on Supabase (production) |
| Frontend | Angular 22 (standalone components, signals), Tailwind CSS v4, Leaflet, Zod |
| Real-time| SignalR (`/orderHub`) |
| Payments | Wompi (Mock / Sandbox / Live), CASH on delivery |
| AI       | DeepSeek chat assistant with real menu context |

## Architecture

**Backend** — 4 projects in `backend/`:

```
Restaurante.Api            ASP.NET Core host: controllers, middleware, SignalR hub, config
Restaurante.Application    CQRS commands/queries + handlers, FluentValidation validators,
                           pure business rules (GeoHelper, API contracts)
Restaurante.Domain         Entities (User, Restaurant, MenuItem, Order, Rider, Coupon, ...),
                           enums, order status state machine, BusinessHoursHelper
Restaurante.Infrastructure EF Core DbContext + migrations, repositories, PaymentService,
                           StorageService (Supabase/local fallback), AIService
Restaurante.Tests          xUnit + NSubstitute unit tests for business rules
```

- **CQRS**: every feature folder exposes `Commands/` and `Queries/` with MediatR handlers that talk
  to repositories through `Application/Interfaces` (no EF types leak into Application).
- **Order state machine**: `Pending → Confirmed → Preparing → Ready → AssignedToRider →
  OutForDelivery → Delivered`; `Cancelled` from any non-terminal state. Same-status updates and
  transitions from terminal states are rejected. Lives in
  `UpdateOrderStatusCommandHandler` (Application/Features/Orders/Commands).
- **Server-side pricing**: order `Total` is always computed by the server
  (`subtotal + DeliveryFee − DiscountAmount`); the client never sends prices or fees.
- **Validation pipeline**: FluentValidation validators live next to the DTOs; handlers return
  `ApiResponse<T>.Ok/Fail` (business failures are HTTP 200 with `success: false`).

**Frontend** — Angular 22 standalone + signals in `frontend/src/app`:

```
core/services      HTTP services (order, cart, payment, review, address, ai, signalr, ...)
core/interceptors  api-response interceptor unwraps { data } envelopes
core/utils         pure helpers (business-hours, browse filters, dashboard metrics, ...)
features/          customer (browse, restaurant view, checkout, tracking, profile)
                    + restaurant (dashboard, orders, menu manager, coupons, settings)
shared/ui          loading / error / empty-state components
```

## Folder structure

```
├── backend/
│   ├── Restaurante.sln
│   ├── Restaurante.{Api,Application,Domain,Infrastructure,Tests}/
│   └── run.sh                     production runner (Supabase/PostgreSQL)
├── frontend/                      Angular 22 app (npm)
├── supabase/                      Supabase SQL snippets (prod schema)
└── README.md
```

## Setup

### Backend (development — SQLite)

```bash
cd backend
export ASPNETCORE_ENVIRONMENT=Development        # PowerShell: $env:ASPNETCORE_ENVIRONMENT="Development"
dotnet restore
dotnet ef database update --project Restaurante.Infrastructure --startup-project Restaurante.Api
dotnet run --project Restaurante.Api             # http://localhost:5001 (frontend dev expects 5001)
```

> The DB provider is switched in `Restaurante.Infrastructure/DependencyInjection.cs`:
> `ASPNETCORE_ENVIRONMENT == "Development"` → SQLite (`Data Source=restaurante.db`);
> anything else → PostgreSQL via `ConnectionStrings:SupabaseConnection`.

### Frontend

```bash
cd frontend
npm install
npm start        # http://localhost:4200 — proxies /api/* to http://localhost:5001 (proxy.conf.json)
```

### Seed the demo data

1. Log in (or register) any user, then call `POST /api/seed` with a Bearer token
   (the endpoint is `[Authorize]` but has no role restriction — it is a demo-only endpoint).
2. It creates 3 restaurants with menus, business hours, delivery settings, coupons,
   a rider, a customer address and one **delivered** demo order (full status history,
   CASH payment, coupon applied, review) so the dashboards look alive.

To re-seed from scratch: delete `backend/Restaurante.Api/restaurante.db` and re-run the EF migration.

## Demo credentials

| Role             | Email                    | Password   |
|------------------|--------------------------|------------|
| Restaurant owner | `demo@restaurante.app`   | `Demo123!` |
| Customer         | `cliente@restaurante.app`| `Demo123!` |
| Rider            | `rider@restaurante.app`  | `Demo123!` |

Demo coupon: `WELCOME10` (10% off at La Casa del Taco, min. order $80).

## Configuration (secrets)

Everything lives in `backend/Restaurante.Api/appsettings.json` (and
`appsettings.Development.json`); replace the `CHANGE_ME` placeholders — or better, use
`dotnet user-secrets` / environment variables:

| Key | Purpose |
|-----|---------|
| `Jwt:SecretKey` | JWT signing key (any long random string) |
| `DeepSeek:ApiKey` | AI chat assistant |
| `Wompi:PublicKey` / `Wompi:PrivateKey` / `Wompi:WebhookSecret` | Wompi payment gateway |
| `Supabase:Url` / `Supabase:ServiceRoleKey` | Storage (images) — falls back to local `wwwroot/uploads` when `CHANGE_ME` |
| `ConnectionStrings:SupabaseConnection` | PostgreSQL connection (production) |

**PaymentProvider modes** (`PaymentProvider:Mode`):

- `Mock` (default) — fake successful transactions (`TXN-...`), no network calls; CASH always settles
  on delivery without touching the gateway.
- `Sandbox` / `Live` — real Wompi transactions; requires the PrivateKey and a **card token**
  (checkout tokenizes the card via Wompi.js client-side, `acceptance_token` optional).
- Webhook: Wompi sends `POST /api/payments/webhook` with header `x-signature` =
  HMAC-SHA256 of the raw body using `Wompi:WebhookSecret`, event `transaction.updated`.

## SignalR events (`/orderHub`)

| Event | Payload | Listeners |
|-------|---------|-----------|
| `NewOrder` | `OrderDto` | restaurant group `restaurant_{id}` |
| `OrderUpdated` | `OrderDto` | restaurant group `restaurant_{id}` |
| `OrderStatusChanged` | `OrderDto` | customer order group `order_{id}` |
| `RiderLocationUpdated` | `{ latitude, longitude }` | order group `order_{id}` (live map) |

## Tests

```bash
# Backend — unit tests over pure business rules (business hours, state machine, order
# creation, coupons, geo distance, webhook signature)
cd backend && dotnet test

# Frontend — 133+ unit tests (services, utils, guards, UI components)
cd frontend && npx vitest run
```

## Production

1. Provision a Supabase project (PostgreSQL), apply the schema from `supabase/` or run EF
   migrations with the PostgreSQL provider.
2. Set all secrets via environment variables and `ConnectionStrings:SupabaseConnection`
   pointing to Supabase; set `ASPNETCORE_ENVIRONMENT=Production`.
3. Build and run the backend:

```bash
cd backend
./run.sh               # production: PostgreSQL, http://localhost:5000
./run.sh development   # development: SQLite, http://localhost:5001
```

4. Build the frontend for deployment: `cd frontend && npm run build` (output in `dist/`).
   The built app talks directly to the API base URL in `src/environments/environment.ts`.
