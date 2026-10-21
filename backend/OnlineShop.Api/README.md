# OnlineShop.Api

ASP.NET Core (.NET 10) REST API for OnlineShopDotnet, using MongoDB and JWT authentication. The full documentation (setup, Docker, configuration, API overview) is in the root [README](../../README.md).

## Run

```bash
dotnet run
```

The API listens on `http://localhost:5000`. Swagger UI: `http://localhost:5000/swagger` (Development environment).

It needs MongoDB at the address in `appsettings.json` (`MongoDB:ConnectionString`, database `OnlineShopDb`). Missing collections and sample data are created on startup.

## Layers

| Folder | Responsibility |
|--------|----------------|
| `Controllers/` | HTTP endpoints, request validation, status codes |
| `Services/` | business rules |
| `Repositories/` | MongoDB access (one repository per collection) |
| `Models/`, `Dtos/` | domain models and request / response shapes |
| `Middleware/` | token-invalidation check and ObjectId validation |
| `Migrations/` | `MigrationRunner` and `SeedData` |

## Tests

From the repository root: `dotnet test`. The tests live in `../OnlineShop.Tests`.
