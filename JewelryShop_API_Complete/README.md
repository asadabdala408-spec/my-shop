# Jewelry Shop Backend API

A professional ASP.NET Core Web API backend for managing a jewelry shop system. The API supports authentication, product catalog management, shopping cart operations, order checkout, and admin order management.

## Technology Stack

- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL / Neon
- JWT Authentication
- Swagger / OpenAPI
- Role-based authorization

## Main Features

- User registration and login
- JWT-based authentication
- Admin and customer roles
- Category management
- Product management
- Product image upload
- Shopping cart management
- Order checkout
- Order status management
- Admin dashboard summary endpoint
- Swagger API documentation
- Database seeding for admin and sample catalog data

## Project Structure

```text
Jewelryshop.Api
├── Controllers
│   ├── AdminController.cs
│   ├── AuthController.cs
│   ├── CartController.cs
│   ├── CategoriesController.cs
│   ├── OrdersController.cs
│   └── ProductsController.cs
├── Data
│   ├── AppDbContext.cs
│   └── DatabaseSeeder.cs
├── Docs
│   ├── DEPLOYMENT_PREPARATION.md
│   ├── FINAL_TESTING_CHECKLIST.md
│   ├── FRONTEND_INTEGRATION_GUIDE.md
│   └── JewelryShop.Api.postman_collection.json
├── DTOs
│   ├── AuthDtos.cs
│   └── CatalogDtos.cs
├── Migrations
├── Models
│   ├── CartItem.cs
│   ├── Category.cs
│   ├── Order.cs
│   ├── OrderItem.cs
│   ├── Product.cs
│   └── User.cs
├── Services
│   ├── JwtService.cs
│   └── PasswordService.cs
├── Program.cs
└── appsettings.json
```

## Configuration

The application uses PostgreSQL through a connection string named `DefaultConnection`.

For local development, store sensitive database credentials in .NET user-secrets instead of `appsettings.json`.

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=your-host;Database=neondb;Username=your-user;Password=your-password;SSL Mode=Require;Channel Binding=Require"
```

The JWT settings are configured under:

```json
"Jwt": {
  "Issuer": "Jewelryshop.Api",
  "Audience": "Jewelryshop.Client",
  "Key": "replace-this-with-a-long-secure-secret-key-that-is-at-least-32-characters"
}
```

For production, replace the JWT key with a secure secret stored outside source control.

## Database Setup

Create the initial migration:

```powershell
dotnet ef migrations add InitialCreate
```

Apply migrations to the database:

```powershell
dotnet ef database update
```

The project includes `DatabaseSeeder`, which automatically creates an admin user and sample catalog data when the API starts.

## Run the API

```powershell
dotnet run
```

Swagger will be available at:

```text
http://localhost:5286/swagger
```

or:

```text
https://localhost:7274/swagger
```

## Default Admin Account

The seeded admin account is:

```text
Email: admin@jewelryshop.com
Password: Admin12345!
```

Use this account to login and access admin-only endpoints.

## Swagger Authorization

1. Open Swagger.
2. Call `POST /api/auth/login`.
3. Copy the returned JWT token.
4. Click `Authorize`.
5. Enter the value in this format:

```text
Bearer YOUR_TOKEN_HERE
```

## API Endpoints

### Auth

```http
POST /api/auth/register
POST /api/auth/login
```

### Categories

```http
GET    /api/categories
GET    /api/categories/{id}
POST   /api/categories
PUT    /api/categories/{id}
DELETE /api/categories/{id}
```

### Products

```http
GET    /api/products
GET    /api/products/{id}
POST   /api/products
POST   /api/products/{id}/image
PUT    /api/products/{id}
DELETE /api/products/{id}
```

Product image upload uses `multipart/form-data` with a file field named `file`.

Allowed image formats:

- JPG
- JPEG
- PNG
- WEBP

Uploaded images are served from:

```text
/uploads/products/{fileName}
```

### Cart

```http
GET    /api/cart
POST   /api/cart
PUT    /api/cart/{id}
DELETE /api/cart/{id}
```

### Orders

```http
GET  /api/orders
GET  /api/orders/{id}
POST /api/orders/checkout
PUT  /api/orders/{id}/status
```

### Admin

```http
GET /api/admin/dashboard
```

## Testing Flow

1. Run the API.
2. Open Swagger.
3. Login using the admin account.
4. Authorize Swagger with the JWT token.
5. Test categories and products.
6. Register a customer account.
7. Login as customer.
8. Add products to cart.
9. Checkout order.
10. Login as admin and update the order status.
11. Login as admin and test `GET /api/admin/dashboard`.

## Extra Documentation

- `Docs/FINAL_TESTING_CHECKLIST.md`
- `Docs/FRONTEND_INTEGRATION_GUIDE.md`
- `Docs/DEPLOYMENT_PREPARATION.md`
- `Docs/JewelryShop.Api.postman_collection.json`

## Security Notes

- Do not commit real database passwords to source control.
- Use .NET user-secrets for local development.
- Rotate exposed database credentials before production deployment.
- Replace the default JWT key before production.
- Replace the default seeded admin password before production.

## Status

The backend is ready for local testing and demonstration with Swagger and Neon PostgreSQL.
