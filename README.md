# ContosoCommerce — .NET Framework 4.8 Web API

A backend CRUD API for an e-commerce platform
built on ASP.NET Web API 2 targeting
.NET Framework 4.8. Designed to demonstrate
a phased .NET 4.x to .NET 8.0 migration.

## Solution Structure

```
ContosoCommerce/
  ContosoCommerce.sln
  ContosoCommerce.Api/        # Web API host
  ContosoCommerce.Core/       # Shared DTOs,
                              # interfaces, enums
  ContosoCommerce.Data/       # EF6 DbContext
                              # and entities
  ContosoCommerce.Users/      # Module 1: Users
                              # & Authentication
  ContosoCommerce.Inventory/  # Module 2:
                              # Products & Stock
  ContosoCommerce.Orders/     # Module 3: Orders
                              # & Payments
  ContosoCommerce.Reporting/  # Module 4:
                              # Reports & Analytics
```

## Projects

### ContosoCommerce.Api
The main Web API host. Uses `Global.asax`,
`web.config`, and `WebApiConfig.cs` for route
registration. Registers all controllers and
wires up dependency injection via
**Unity Container**.

### ContosoCommerce.Core
Shared DTOs, interfaces, enums, and constants
used across all modules:
- `ApiResponse<T>`, `PagedResult<T>` — response
  wrappers
- `OrderStatus`, `UserRole`, `StockLevel` — enums
- `IAuditService`, `ICacheService`,
  `INotificationService` — cross-cutting
  interfaces
- `EntityNotFoundException`,
  `BusinessRuleException` — custom exceptions

### ContosoCommerce.Data
Data access layer using **Entity Framework 6**
with a single shared `CommerceDbContext`.
Uses `Database.SetInitializer` with seed data.

### ContosoCommerce.Users (Module 1)
- `UsersController` — CRUD for users
- `AuthController` — login returns a custom
  DB-stored token (not JWT)
- `UserService` — password hashing with the
  deprecated `FormsAuthentication` API
- `TokenAuthorizeAttribute` — custom auth filter
  checking DB tokens on every request
- `UserRepository` — raw SQL queries via
  `Database.SqlQuery<T>()`

### ContosoCommerce.Inventory (Module 2)
- `ProductsController` — CRUD with image upload
  (byte[] stored in DB)
- `CategoriesController` — hierarchical
  categories (self-referencing FK)
- `StockController` — stock level management
- `InventoryService` — uses `System.Drawing`
  for thumbnails
- `StockMonitorService` — background stock check
  via `System.Timers.Timer`
- Uses `HttpContext.Current` in the service layer

### ContosoCommerce.Orders (Module 3)
- `OrdersController` — create, get, list, update
- `PaymentsController` — mock payment processing
- `OrderService` — uses `TransactionScope` for
  multi-table consistency
- Uses `Task.Factory.StartNew` with
  `LongRunning` for background payment
  processing
- `OrderNotificationHandler` —
  `DelegatingHandler` that sends email via
  `SmtpClient` (deprecated in .NET 8)
- Reads config via `ConfigurationManager`

### ContosoCommerce.Reporting (Module 4)
- `ReportsController` — sales, inventory, and
  user activity reports
- Legacy XML endpoint via `XmlSerializer`
- `ReportService` — queries across all modules
  via shared DbContext with complex LINQ
- Uses `System.Drawing` for chart images
- Caches with `MemoryCache` and
  `HttpRuntime.Cache`
- `ExportService` — CSV generation via
  `StringBuilder`
- `ReportScheduler` — pre-generates reports via
  `System.Threading.Timer`

## Cross-Cutting Concerns

| Concern                  | Pattern Used              |
|--------------------------|---------------------------|
| Shared DbContext         | Single EF6 context        |
| Auth filter              | `[TokenAuthorize]`        |
| Audit logging            | `HttpContext.Current`      |
| Logging                  | log4net via web.config    |
| DI container             | Unity Container           |
| Configuration            | `ConfigurationManager`    |
| Request timing           | Custom `HttpModule`       |
| Date parsing             | Custom model binder       |
| Assembly redirects       | web.config runtime section|
| Config transforms        | Web.Debug/Release.config  |

## Prerequisites

- Visual Studio 2019 or later
- .NET Framework 4.8 SDK
- SQL Server LocalDB (auto-created by EF6)

## How to Run

1. Open `ContosoCommerce.sln` in Visual Studio
2. Restore NuGet packages
3. Set `ContosoCommerce.Api` as startup project
4. Press F5 to run
5. The database is created and seeded on
   first launch

## API Endpoints

### Auth
- `POST /api/auth/login` — authenticate

### Users
- `GET /api/users` — list users
- `GET /api/users/{id}` — get user
- `POST /api/users` — create user
- `PUT /api/users/{id}` — update user
- `DELETE /api/users/{id}` — delete user

### Products
- `GET /api/products` — list products
- `GET /api/products/{id}` — get product
- `POST /api/products` — create product
- `PUT /api/products/{id}` — update product
- `DELETE /api/products/{id}` — delete product
- `GET /api/products/{id}/image` — get image

### Categories
- `GET /api/categories` — list categories
- `POST /api/categories` — create category

### Stock
- `GET /api/stock/{id}/level` — get stock level
- `PUT /api/stock/{id}/adjust` — adjust stock
- `GET /api/stock/low` — get low-stock products

### Orders
- `GET /api/orders/{id}` — get order
- `GET /api/orders/user/{userId}` — user orders
- `POST /api/orders` — create order
- `PUT /api/orders/{id}/status` — update status

### Payments
- `POST /api/payments/order/{orderId}` — process

### Reports
- `GET /api/reports/sales` — sales report
- `GET /api/reports/inventory` — inventory report
- `GET /api/reports/users` — user activity report
- `GET /api/reports/sales/chart` — chart image
- `GET /api/reports/sales/csv` — CSV export
- `GET /api/reports/inventory/csv` — CSV export
- `GET /api/reports/sales/xml` — XML export

## Seed Data

The database is seeded with:
- 4 users (admin, inventory manager, customer,
  report viewer) — all with password `P@ssw0rd!`
- 3 categories (Electronics, Clothing, Phones)
- 5 products
- 1 completed order with 2 items and payment
