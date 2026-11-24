# MiniShop 3

A bilingual ASP.NET Core 8 MVC ecommerce starter with a product-led storefront, admin tools and a self-contained customer assistant. English and Arabic, including RTL layouts, are supported.

## Run

Install the .NET 8 SDK, unzip, then run from the `MiniShop` folder:

```sh
dotnet build
dotnet run --project src/MiniShop.Web --urls http://localhost:5000
```

Open http://localhost:5000. A fresh SQLite database is created and seeded automatically. There is no frontend build step, external font dependency, API key or paid service.

**Upgrading from an earlier MiniShop:** the schema changed in this version. `EnsureCreated` does not upgrade an existing database. Use a separate fresh database for this demo, or back up your old `src/MiniShop.Web/minishop.db` before removing it. Removing it deletes existing demo users, products and orders. Production upgrades need reviewed EF migrations and a backup/restore plan. The source zip contains no database.

## Demo accounts

| Role | Email | Password |
| --- | --- | --- |
| Admin | admin@minishop.local | `Admin123!` in Development only |
| Customer | demo@minishop.local | Demo123! |

These are development-only credentials. When `ASPNETCORE_ENVIRONMENT` is not `Development`, the seeded admin password comes from the `ADMIN_SEED_PASSWORD` configuration value (environment variable); if it is unset, a random password is generated and the admin account stays locked. Set a strong `ADMIN_SEED_PASSWORD` before any deployment, keep it out of source control, and remove or replace both accounts before real use. This is a starter, not a production-ready store.

## Deploy with Docker (for example on Render)

The included `Dockerfile` builds and runs the app on port 8080. On Render: create a Web Service from this repository (Docker runtime), set the environment variable `ADMIN_SEED_PASSWORD` to a strong secret, and deploy. The free instance sleeps after inactivity (first request after a sleep takes about a minute) and its filesystem is ephemeral, so the SQLite demo database reseeds on each restart or redeploy. A paid instance or an external database is needed for persistent data.

## New in this version

### Discount coupons

- Admin UI: `/Admin/Shop/Coupons`.
- Create/edit codes, percentage discounts (1-90%), minimum basket amounts, expiry in UTC, and active/inactive state. Disabling a code replaces deletion and keeps order history readable.
- `WELCOME10` gives 10% off with no minimum. Seed expiry is one year after the fresh database is created.
- Codes are case-insensitive on checkout. One code per order, no stacking. They can be reused; there is no per-user or global redemption limit in this demo.
- Codes are validated server-side both when applied and when the order is submitted. Invalid, expired, inactive and minimum-spend failures are shown in the selected language.
- Totals use current catalog prices, not stale session prices. Monetary discounts are rounded to two decimal places, away from zero.
- Orders retain subtotal, discount, code and final total. The welcome banner is hidden when that code is no longer valid for the advertised offer.

### Built-in customer assistant

The floating MiniShop guide is a deterministic, keyword/knowledge-based assistant, **not generative AI**. It works without an LLM, external APIs, credentials or fees. Its label makes that distinction clear.

It answers in the selected UI language about:

- Catalog names, descriptions, current prices and stock.
- Currently valid coupon codes and minimum-spend requirements.
- The signed-in customer's own order statuses and totals. Order numbers are checked only within that user's account.
- Demo shipping, returns and payment limitations.

Try `headphones price`, `Any discount codes?`, `Track my orders`, `How does delivery work?`, `سعر سماعات`, `كوبونات خصم` or `تتبع طلباتي`.

Unknown questions return a scope explanation rather than an invented answer. Answers are inserted as plain text to prevent HTML/script execution. The endpoint is read-only and protected by antiforgery validation. Chat is not persisted. Product matching and intent recognition are intentionally simple; synonyms and complex multi-part questions may need rephrasing. Quick-question chips are available in both languages.

### Storefront redesign

- Search-first header, category navigation, editorial split hero and a restrained green/ivory palette.
- Original local SVG product illustrations replace emoji product tiles. They are illustrative, not product photography. New admin-created products use the closest name-based illustration or a generic stand illustration.
- Three collections (desk, audio, displays), search, price sorting and empty states. Categories and artwork are inferred from English names in this sample rather than managed as separate entities.
- Product details, saved-item hearts, stock indicators and clear prices.
- Responsive two-column mobile product grid, RTL layout, keyboard focus, form labels, reduced-motion support and chat announcements.
- New checkout summary separates subtotal, discount and demo shipping. Real card fields have been removed completely.

The layout draws on patterns observed on the Apple accessories store and IKEA storefront: product-led editorial sections, clear navigation, consistent merchandise cards and service/support entry points. No brand logos, marketing claims, reviews or product photography were copied.

Design references inspected November 24, 2026:
- https://www.apple.com/shop/accessories/all
- https://www.ikea.com/us/en/

### Wishlist

Sign in and tap a heart to save or unsave a product. Saved items are persisted in SQLite per account and available at `/Wishlist`. No cross-user sharing is implemented.

### Order tracking

- Orders start as Confirmed.
- Admins advance them through Processing, Shipped and Delivered at `/Admin/Shop/Orders`.
- Skipping/backward transitions are rejected server-side.
- Customers see a translated progress timeline in My orders.
- These are manually updated **demo statuses**, not live courier tracking or delivery promises.

### Cart and inventory

Cart quantities can be edited. Additions are bounded by current stock. Order submission rechecks current prices and stock, and stock deduction plus order creation are committed in one database transaction. Conditional stock updates protect against overselling. Unavailable items block submission rather than silently charging a different total.

## Clean Architecture

```text
src/
  MiniShop.Domain/          Entities, repository contracts, password hashing
  MiniShop.Application/     Catalog, cart, accounts, orders, coupons, wishlist,
                            tracking and knowledge-assistant use cases
  MiniShop.Infrastructure/  EF Core SQLite repositories and sample data
  MiniShop.Web/             MVC controllers, Razor views, resources, local SVG/CSS/JS
```

Domain has no project dependencies. Application depends on Domain. Infrastructure implements Domain contracts. Web composes the dependency graph and adapts HTTP, authentication and session concerns. Controllers do not access EF directly. Coupon calculations, assistant intent handling and ordering rules live in Application services. All writes use POST plus antiforgery checks; admin actions require the AdminOnly policy. Order information is scoped by the authenticated user's identity.

## Verification

Build verified with zero warnings and zero errors. Browser regression checks cover login, registration, cart updates, coupon application, mock checkout, persisted order totals, wishlist, admin pages, status transitions, assistant responses, account isolation, Arabic RTL, mobile overflow and antiforgery rejection.

A reproducible browser smoke test is included at `tests/smoke.py`. It builds and runs a temporary copy with a fresh database, leaving your working database untouched. Requires Python, Playwright, Chrome and the .NET 8 SDK:

```sh
python -m pip install playwright
python tests/smoke.py
```

Set `DOTNET` and `CHROME` environment variables if the executables are not on your PATH. Screenshots from the test go to `test-output/`, which is ignored by git. Do not run two smoke tests against the same port at once.

## Explicit demo boundaries

No money is charged, no card data is requested, and no payment provider, courier, email delivery or external AI is connected. Checkout shipping is 0 EGP only as a demo value. No real delivery or returns policy is promised. No real refunds or fulfilment actions happen.

The app uses `EnsureCreated`, cookie authentication and in-memory session storage. Production work still includes HTTPS/security headers, ASP.NET Identity or equivalent password reset/email verification/lockout, shared data protection and session storage, pagination, rate limiting, observability, proper migrations, checkout idempotency, tax/shipping rules and approved store policies.

## Suggested next steps

- Real payment-gateway test-mode integration with signed webhooks and idempotent order creation.
- Real shipping calculations, addresses, tax breakdowns and courier tracking.
- Managed categories/images, accessible image uploads and product variants.
- Coupon redemption limits, per-user eligibility, audit history and fixed-value discounts.
- Persistent carts, compare products, verified reviews and recent-view history.
- Email receipts, password reset, verified accounts and admin audit logs.
- Optional retrieval-based generative assistant with explicit provider configuration, cost controls and privacy review. Keep account isolation and a strict read-only tool scope.

## New in this version (shipping, cash on delivery and more)

### Delivery fee from the address
The checkout asks for name, mobile number, governorate, city and street. The governorate decides the fee, shown live (page reloads when the governorate or payment option changes, typed fields are kept) with an estimated delivery window.

The model follows the zone-based approach published by Amazon.eg for marketplace shipping (destination zones such as Cairo/Giza/Qalyoubia metro, other metro, non-metro and remote areas, a flat rate per shipment, and a free-shipping threshold; source: amazon.eg "Seller Shipping Rates and Times" help page) and Jumia Egypt's free-delivery-above-a-basket-amount and cash-on-delivery options (Jumia's own shipping policy page publishes no fee table, so no Jumia numbers are copied).

- 27 governorates are grouped into 4 zones. Seed values are **demo numbers, not real carrier prices**: Greater Cairo 30 EGP (free from 500), Alexandria/Delta/Canal 45 (free from 750), Upper Egypt 60 (free from 1000), Remote areas 90 (free from 1500).
- Free delivery is judged on the basket **after the coupon**. A hint shows how much more to add to unlock it.
- Fee, zone, address, payment method and payment status are stored on the order. Changing fees later never alters old orders.
- The server recalculates the fee when the order is placed. The page is never trusted.

### Admin: `/Admin/Shop/Shipping`
Edit each zone (EN/AR name, fee, free-delivery threshold, min/max days, active), add zones, move a governorate to another zone, and set cash on delivery (enable, fee, maximum order value). A deactivated zone blocks delivery to its governorates. Changes affect new checkouts only.

### Cash on delivery (demo)
A second payment option next to the demo online payment. It adds the configurable COD fee (seed 15 EGP, cap 20,000 EGP). The order is created with payment status "Due on delivery (demo)", and becomes "Collected (demo)" when the admin moves it to Delivered. No money is collected or recorded anywhere.

### Extra features
- **Free-delivery nudge** at checkout ("Add X EGP more for free delivery").
- **Address prefill** from the customer's last order.
- **Customer order cancellation** while the order is still "Confirmed". Stock returns in the same transaction. Demo payments show "Refunded (demo)". Admin cannot move a cancelled order forward.
- The assistant now answers delivery and cash-on-delivery questions from the live admin settings.

### What is mocked
Delivery fees and estimates (demo values, no courier, no address validation or geocoding), online payment, cash collection, refunds and order tracking. Governorate-level pricing only; city or postcode-level pricing is not modeled.

### Next steps
Express/same-day delivery options, per-city fees, weight or size surcharges, courier integration, real payment gateway (Paymob/Fawry), coupon redemption limits, product reviews, order invoices (PDF), email/WhatsApp order notifications, EF migrations, inventory alerts for admin.

**Upgrading:** the schema changed again. Delete the old `src/MiniShop.Web/minishop.db`; it is recreated and seeded on start.

