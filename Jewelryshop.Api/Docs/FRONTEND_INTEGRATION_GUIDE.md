# Frontend Integration Guide

This guide explains how a frontend application can connect to the Jewelry Shop Backend API.

## Base URL

Local development URLs:

```text
http://localhost:5286
https://localhost:7274
```

Use one base URL consistently in the frontend.

## Authentication Flow

### Login

Request:

```http
POST /api/auth/login
```

Body:

```json
{
  "email": "admin@jewelryshop.com",
  "password": "Admin12345!"
}
```

Response:

```json
{
  "token": "JWT_TOKEN",
  "userId": "USER_ID",
  "fullName": "System Admin",
  "email": "admin@jewelryshop.com",
  "role": 2
}
```

Store the token safely on the client. For simple development, localStorage can be used. For production, consider more secure storage strategies.

## Sending Authorized Requests

Every protected request should include this header:

```http
Authorization: Bearer JWT_TOKEN
```

Example with JavaScript `fetch`:

```js
const response = await fetch(`${API_BASE_URL}/api/cart`, {
  headers: {
    Authorization: `Bearer ${token}`
  }
});
```

## Public Endpoints

These can be used without login:

```http
GET /api/categories
GET /api/products
GET /api/categories/{id}
GET /api/products/{id}
```

## Customer Flow

1. Register or login.
2. Fetch products.
3. Add product to cart.
4. Fetch cart.
5. Checkout.
6. View orders.

Important endpoints:

```http
POST /api/auth/register
POST /api/auth/login
GET  /api/products
POST /api/cart
GET  /api/cart
POST /api/orders/checkout
GET  /api/orders
```

## Admin Flow

1. Login as admin.
2. Manage categories.
3. Manage products.
4. View all orders.
5. Update order status.
6. View dashboard summary.

Important endpoints:

```http
POST   /api/categories
PUT    /api/categories/{id}
DELETE /api/categories/{id}
POST   /api/products
PUT    /api/products/{id}
DELETE /api/products/{id}
GET    /api/orders
PUT    /api/orders/{id}/status
GET    /api/admin/dashboard
```

## Product Creation Body

```json
{
  "name": "Luxury Watch",
  "description": "Premium watch with elegant design.",
  "price": 499.99,
  "stockQuantity": 12,
  "imageUrl": "https://images.unsplash.com/photo-1523275335684-37898b6baf30",
  "categoryId": 1
}
```

## Cart Body

```json
{
  "productId": 1,
  "quantity": 1
}
```

## Order Status Body

```json
{
  "status": "Completed"
}
```

Valid statuses:

- Pending
- Processing
- Completed
- Cancelled

## Common Errors

### 401 Unauthorized

Possible causes:

- Missing token.
- Token does not start with `Bearer`.
- Token is expired.
- User is not logged in.

### 403 Forbidden

Possible causes:

- Customer is trying to access admin-only endpoint.

### 400 Bad Request

Possible causes:

- Invalid category ID.
- Invalid product ID.
- Quantity is zero or negative.
- Not enough product stock.

## Recommended Frontend Pages

- Home page
- Products page
- Product details page
- Cart page
- Checkout page
- Login page
- Register page
- Admin dashboard
- Admin categories page
- Admin products page
- Admin orders page
