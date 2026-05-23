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
GET /api/orders/admin
```

The admin dashboard returns production-oriented analytics:

- User/customer/admin totals
- Product/category/order totals
- Pending/completed order counts
- Completed-order revenue
- Low-stock count and low-stock product list
- Recent orders
- Best-selling products
- Order status breakdown

## Production Readiness Checklist

Before going live, configure these values outside source control:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=your-production-host;Database=your-db;Username=your-user;Password=your-password;SSL Mode=Require;Trust Server Certificate=true"
dotnet user-secrets set "Jwt:Key" "use-a-long-random-production-secret-with-at-least-32-characters"
dotnet user-secrets set "Cloudinary:CloudName" "your-cloud-name"
dotnet user-secrets set "Cloudinary:ApiKey" "your-api-key"
dotnet user-secrets set "Cloudinary:ApiSecret" "your-api-secret"
```

Recommended production tasks:

- Replace the default seeded admin password.
- Store secrets in the hosting provider environment variables.
- Update CORS origins to your deployed frontend domain only.
- Run migrations against the production database.
- Enable HTTPS on backend and frontend.
- Use a real payment provider before accepting online payments.
- Add email notifications for order confirmation and status changes.
- Monitor logs, failed requests, and database connection health.

## Payment Integration Plan

Checkout currently stores a payment method placeholder. To make payments real:

- Use Stripe, PayPal, or a mobile-money provider.
- Create a payment intent/session before final order confirmation.
- Store provider transaction ID on the order.
- Mark the order as paid only after provider confirmation/webhook.
- Keep cash-on-delivery available as a manual payment option.

## Email Integration Plan

Add an email provider such as SendGrid, Mailgun, SMTP, or Resend.

Suggested emails:

- Welcome email after registration.
- Order confirmation email after checkout.
- Admin notification when a new order is created.
- Customer notification when order status changes.

Store provider API keys in environment variables, not in `appsettings.json`.

## Deployment Plan

Backend:

- Deploy the ASP.NET Core API to Azure App Service, Render, Railway, Fly.io, or similar.
- Configure `ConnectionStrings__DefaultConnection`, `Jwt__Key`, `Jwt__Issuer`, `Jwt__Audience`, and Cloudinary settings as environment variables.
- Confirm Swagger is disabled or protected if needed.

Frontend:

- Deploy the React/Vite app to Netlify, Vercel, or static hosting.
- Set `VITE_API_BASE_URL` to the deployed backend URL.
- Rebuild frontend after changing `VITE_API_BASE_URL`.

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
