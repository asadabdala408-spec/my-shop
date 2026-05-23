# Deployment Preparation Guide

This guide lists the steps needed before deploying the Jewelry Shop Backend API.

## 1. Rotate Sensitive Credentials

If a database password was shared in chat or committed anywhere, rotate it in Neon before production.

## 2. Configure Environment Variables

Do not store production secrets in `appsettings.json`.

Recommended production variables:

```text
ConnectionStrings__DefaultConnection
Jwt__Issuer
Jwt__Audience
Jwt__Key
ASPNETCORE_ENVIRONMENT
```

Example values:

```text
ASPNETCORE_ENVIRONMENT=Production
Jwt__Issuer=Jewelryshop.Api
Jwt__Audience=Jewelryshop.Client
Jwt__Key=A_LONG_RANDOM_SECRET_KEY_FOR_PRODUCTION
```

## 3. Database Migration

Before deployment, apply migrations to the production database:

```powershell
dotnet ef database update
```

If the hosting provider runs migrations automatically, make sure the connection string is configured first.

## 4. CORS Setup

If a separate frontend application will call this API from another domain, configure CORS in `Program.cs`.

Example frontend domains:

```text
http://localhost:3000
https://your-frontend-domain.com
```

## 5. Swagger in Production

Swagger is currently enabled only in development.

Recommended:

- Keep Swagger disabled in production unless needed.
- If enabled, protect it behind authentication or a private network.

## 6. Default Admin Account

The current seeded admin is:

```text
admin@jewelryshop.com
Admin12345!
```

Before production:

- Change the default admin password.
- Or remove default admin seeding and create an admin manually.

## 7. Logging and Monitoring

Recommended production monitoring:

- Application logs
- Database errors
- Authentication failures
- Order checkout failures
- API response time

## 8. Final Production Checklist

- `dotnet build` succeeds.
- Database migrations applied.
- Production connection string configured.
- JWT key replaced with secure value.
- CORS configured for frontend domain.
- Default admin password changed.
- Swagger decision made for production.
- API tested with Postman or Swagger.
