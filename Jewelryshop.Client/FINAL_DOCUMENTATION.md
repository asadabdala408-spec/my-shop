# Jewelry Shop Full-Stack Documentation

## Projects

```text
Jewelryshop.Api       ASP.NET Core Web API backend
Jewelryshop.Client    React + Vite frontend
```

## Frontend features

- Product listing
- Product details modal
- Search products
- Filter by category
- Filter by max price
- Filter in-stock products
- Sort by name, price, and stock
- Cart page
- Checkout
- Orders page
- Admin login
- Admin dashboard
- Create categories
- Create products
- Edit products
- Delete products
- Upload product images to Cloudinary

## Backend features

- ASP.NET Core Web API
- PostgreSQL with EF Core
- Products API
- Categories API
- Cart API
- Orders API
- Admin dashboard API
- Cloudinary image upload
- Cloudinary image delete support
- Swagger documentation

## Important local URLs

```text
Backend:  http://localhost:5286
Swagger:  http://localhost:5286/swagger
Frontend: http://localhost:5173
```

## Admin login

```json
{
  "email": "admin@jewelryshop.com",
  "password": "Admin12345!"
}
```

## Common commands

Backend:

```powershell
dotnet run
dotnet build
```

Frontend:

```powershell
npm.cmd install
npm.cmd run dev
npm.cmd run build
```

## Notes

This project currently has JWT authorization removed for easier testing. Endpoints are public in development mode.
