# OnlineShopDotnet

A full-stack **e-commerce application**: an **ASP.NET Core** REST API backed by **MongoDB** with **JWT authentication**, and a **Next.js** storefront with admin pages.

The backend follows a layered design (controllers → services → repositories), seeds sample data on first run, and ships with a large automated test suite. The whole stack can be started with Docker Compose.

---

## Features

**Authentication & authorization**
- Registration and login
- BCrypt password hashing
- JWT bearer tokens
- Role-based authorization (`Admin`, `User`)
- Logout with real token invalidation (invalidated tokens are stored and rejected on later requests)

**Shop**
- Products and categories
- Cart and wishlist (the wishlist includes product details)
- Orders: customers see their own orders and can cancel `Pending` / `Processing` orders
- Coupons: percentage or fixed value, expiration, usage limits, optional per-user assignment
- Product reviews
- Inventory restock / reduce with inventory events

**Admin**
- Manage products, categories, users and orders from the storefront's admin pages

**Platform**
- MongoDB with the repository pattern
- Automatic collection creation and seed data on startup
- Swagger UI (Development environment)
- Postman collection
- Docker Compose setup for the API and the frontend

---

## Tech stack

| Area | Technology |
|------|------------|
| Backend | ASP.NET Core (.NET 10) |
| Database | MongoDB (official .NET driver) |
| Auth | JWT bearer + BCrypt.Net |
| API docs | Swagger (Swashbuckle) |
| Frontend | Next.js 16 (App Router), React, TypeScript, Tailwind CSS |
| Tests | xUnit, FluentAssertions, Moq, Mongo2Go (embedded MongoDB) |
| Containers | Docker, Docker Compose |

---

## Project structure

```
.
├── docker-compose.yml
├── OnlineShopDotnet.slnx
├── backend/
│   ├── OnlineShop.Api/
│   │   ├── Controllers/        HTTP endpoints
│   │   ├── Services/           business logic
│   │   ├── Repositories/       MongoDB access
│   │   ├── Models/             domain models
│   │   ├── Dtos/               request / response shapes
│   │   ├── Middleware/         token-invalidation + ObjectId validation
│   │   ├── Migrations/         MigrationRunner + SeedData
│   │   ├── Settings/
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── Dockerfile
│   └── OnlineShop.Tests/       xUnit tests
└── frontend/
    ├── app/                    pages (App Router) and components
    ├── src/lib/api.ts          typed API client
    ├── src/context/            snackbar notifications
    ├── src/store/              client-side auth state
    └── Dockerfile
```

---

## Quick start with Docker

**Prerequisites**
- Docker (Docker Desktop or Docker Engine) with Compose
- MongoDB already running on your machine at `localhost:27017`

The containers use **your existing local MongoDB**. No database container is started.

```bash
# first run (builds both images, takes a few minutes)
docker compose up --build

# afterwards
docker compose up        # start
docker compose stop      # stop, keep the containers
docker compose down      # stop and remove the containers
```

| Service | URL |
|---------|-----|
| Frontend | http://localhost:3000 |
| API | http://localhost:5000 |
| Swagger UI | http://localhost:5000/swagger |

The backend receives its MongoDB settings from environment variables in `docker-compose.yml`, so no code or JSON changes are needed. It reaches the database on your machine through `host.docker.internal`.

The frontend's API URL (`NEXT_PUBLIC_API_URL`) is baked into the JavaScript bundle **at build time**. If you change it, rebuild with `docker compose up --build`.

---

## Running without Docker

**Prerequisites:** .NET 10 SDK, Node.js 20.9+ (Node 22 is used in the Docker image), MongoDB running locally.

**1. Backend**

```bash
cd backend/OnlineShop.Api
dotnet run
```

The API listens on `http://localhost:5000`; Swagger is at `http://localhost:5000/swagger`.

**2. Frontend**

Create `frontend/.env.local`:

```
NEXT_PUBLIC_API_URL=http://localhost:5000/api
```

Then:

```bash
cd frontend
npm install
npm run dev
```

Open http://localhost:3000. Restart the dev server after changing `.env.local`.

---

## Configuration

Settings live in `backend/OnlineShop.Api/appsettings.json`:

```json
"MongoDB": {
  "ConnectionString": "mongodb://localhost:27017",
  "DatabaseName": "OnlineShopDb"
},
"Jwt": {
  "Key": "<at least 32 characters>",
  "Issuer": "...",
  "Audience": "...",
  "ExpiresInMinutes": "60"
}
```

Any setting can be overridden with an environment variable (a double underscore replaces `:`):

| Setting | Environment variable |
|---------|----------------------|
| `MongoDB:ConnectionString` | `MongoDB__ConnectionString` |
| `MongoDB:DatabaseName` | `MongoDB__DatabaseName` |
| `Jwt:Key` | `Jwt__Key` |
| `Jwt:Issuer` | `Jwt__Issuer` |
| `Jwt:Audience` | `Jwt__Audience` |
| `Jwt:ExpiresInMinutes` | `Jwt__ExpiresInMinutes` |

- The JWT key must be at least 32 characters. The key in the repository is for local development only. Use your own secret anywhere else.
- CORS currently allows only `http://localhost:3000`. To serve the frontend from another origin, update the `AllowFrontend` policy in `Program.cs`.

---

## Seed data and default admin

On startup `MigrationRunner` creates any missing collections and `SeedData` fills **empty** collections with sample data (each step is skipped when its collection already has data):

- an admin user
- categories: Electronics, Books, Clothing
- two sample products with initial stock
- coupons `WELCOME10` (10 % off) and `SAVE50` (fixed 50)
- a sample wishlist item, review and order

Default admin account:

```
Email:    admin@shop.com
Password: admin
Role:     Admin
```

Change this password (or remove the account) for anything other than local development.

---

## API overview

Send the token from `POST /api/auth/login` as `Authorization: Bearer <token>`. Swagger lists every route and model and is the source of truth.

| Resource | Endpoints | Access |
|----------|-----------|--------|
| Auth | `POST /api/auth/register`, `POST /api/auth/login`, `POST /api/auth/logout` | public (logout needs a token) |
| Users | `GET /api/users`, `GET /api/users/{id}`, `POST /api/users/{id}/roles`, `DELETE /api/users/{id}` | Admin (`GET {id}`: the user themselves or Admin) |
| Products | `GET /api/products`, `GET /api/products/{id}`, `POST` / `PUT` / `DELETE` | read: public, write: Admin |
| Categories | `GET /api/categories`, `GET /api/categories/{id}`, `POST` / `PUT` / `DELETE` | read: public, write: Admin |
| Cart | `GET /api/cart`, `POST /api/cart/add`, `PUT /api/cart/update`, `DELETE /api/cart/remove`, `DELETE /api/cart/clear` | logged in |
| Wishlist | `GET /api/wishlist`, `POST /api/wishlist/add`, `DELETE /api/wishlist/remove` | logged in |
| Orders | `GET /api/orders`, `GET /api/orders/me`, `GET /api/orders/{id}`, `POST /api/orders`, `PUT /api/orders/{id}`, `POST /api/orders/{id}/cancel`, `DELETE /api/orders/{id}` | logged in (Admin sees all orders) |
| Coupons | `GET /api/coupons/validate/{code}`, `GET /api/coupons`, `GET /api/coupons/me`, `POST /api/coupons/{id}/use`, `POST /api/coupons`, `DELETE /api/coupons/{id}` | validate: public, create/delete: Admin, the rest: logged in |
| Reviews | `GET /api/reviews/{productId}`, `POST /api/reviews`, `DELETE /api/reviews/{id}` | posting needs login, deleting is Admin |
| Inventory | `POST /api/inventory/restock`, `POST /api/inventory/reduce` | Admin |

Request-shape details worth knowing:
- **Cart** `add` / `update` / `remove` take `productId` (and `quantity`) as **query-string parameters**, not a JSON body.
- **Wishlist** `add` / `remove` take a JSON body: `{ "productId": "..." }`.
- `GET /api/orders/me` always returns only the caller's own orders, even for admins.
- Cancelling an order returns `409 Conflict` unless its status is `Pending` or `Processing`.

---

## Postman

A Postman collection with all routes is included in the repository. Routes use `{{baseUrl}}/api/...`. Set these environment variables:

- `baseUrl` = `http://localhost:5000`
- `token` = the JWT returned by login
- `productId`
- `categoryId`

---

## Testing

```bash
dotnet test          # from the repository root
```

- Repository and service tests run against a throwaway embedded MongoDB (Mongo2Go), so your local database is never touched and no MongoDB needs to be running.
- Most controller tests use in-memory fakes of the services.
- Frontend type check (this is also what makes `next build`, and therefore the frontend Docker build, fail): from `frontend/` run `npx tsc --noEmit`.

---

## Troubleshooting

- **The browser shows a CORS or network error.** Usually the API is not running. Check `docker compose logs backend`.
- **The backend logs a MongoDB timeout.** Make sure MongoDB is running on `localhost:27017`.
- **The backend logs `Name or service not known` for `host.docker.internal`.** Add this to the `backend` service in `docker-compose.yml`, then run `docker compose up --force-recreate`:
  ```yaml
  extra_hosts:
    - "host.docker.internal:host-gateway"
  ```
- **The frontend still calls the old API URL.** `NEXT_PUBLIC_API_URL` is fixed at build time. Rebuild with `docker compose up --build`.

---

## Development notes

- Collection names are set in each repository. `Category` and `Order` are singular; the others are plural. Keep `RepositoryTestBase` (test cleanup) in sync if you rename one.
- `MigrationRunner` currently uses the database name `OnlineShopDb` directly instead of reading `MongoDB:DatabaseName`. Keep the setting at `OnlineShopDb`, or update the runner.
- Controller unit tests call actions directly, so `[Authorize]` attributes do not run. Where a test needs a role check, the action also checks `User.IsInRole("Admin")` in code.
- Passwords are always hashed with BCrypt, and logging out stores the token in the `InvalidTokens` collection.

---

## License

MIT License.
