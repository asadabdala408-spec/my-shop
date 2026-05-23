# Final Testing Checklist

Use this checklist before presenting or submitting the Jewelry Shop Backend API.

## 1. Application Startup

- Run the API with `dotnet run`.
- Confirm the API starts without errors.
- Open Swagger at `http://localhost:5286/swagger` or `https://localhost:7274/swagger`.
- Confirm the Swagger `Authorize` button is visible.

## 2. Authentication Testing

### Admin Login

Endpoint:

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

Expected result:

- Status code `200 OK`.
- Response contains a JWT `token`.

### Customer Registration

Endpoint:

```http
POST /api/auth/register
```

Body:

```json
{
  "fullName": "Test Customer",
  "email": "customer@example.com",
  "password": "Customer12345!"
}
```

Expected result:

- Status code `200 OK`.
- Response contains a JWT `token`.
- User role is `Customer`.

## 3. Swagger Authorization

- Click `Authorize`.
- Enter the token in this format:

```text
Bearer YOUR_TOKEN_HERE
```

Expected result:

- Protected endpoints execute without `401 Unauthorized`.

## 4. Category Testing

### Public Category List

Endpoint:

```http
GET /api/categories
```

Expected result:

- Status code `200 OK`.
- Returns category data.

### Admin Create Category

Endpoint:

```http
POST /api/categories
```

Body:

```json
{
  "name": "Watches",
  "description": "Luxury watches and accessories."
}
```

Expected result:

- Status code `201 Created`.
- Category is saved to database.

## 5. Product Testing

### Public Product List

Endpoint:

```http
GET /api/products
```

Expected result:

- Status code `200 OK`.
- Returns product data.

### Admin Create Product

Endpoint:

```http
POST /api/products
```

Body:

```json
{
  "name": "Luxury Watch",
  "description": "Premium watch with elegant design.",
  "price": 499.99,
  "stockQuantity": 12,
  "imageUrl": "https://images.unsplash.com/photo-1523275335684-37898b6baf30",
  "categoryId": "PASTE_CATEGORY_ID_HERE"
}
```

Expected result:

- Status code `201 Created`.
- Product is saved to database.

## 6. Cart Testing

Use a customer token.

### Add Product to Cart

Endpoint:

```http
POST /api/cart
```

Body:

```json
{
  "productId": "PASTE_PRODUCT_ID_HERE",
  "quantity": 1
}
```

Expected result:

- Status code `200 OK`.
- Product is added to cart.

### Get Cart

Endpoint:

```http
GET /api/cart
```

Expected result:

- Status code `200 OK`.
- Cart items are returned.

## 7. Order Testing

Use a customer token.

### Checkout

Endpoint:

```http
POST /api/orders/checkout
```

Expected result:

- Status code `201 Created`.
- Order is created.
- Cart is cleared.
- Product stock is reduced.

### Get Customer Orders

Endpoint:

```http
GET /api/orders
```

Expected result:

- Status code `200 OK`.
- Customer sees only their own orders.

## 8. Admin Order Management

Use an admin token.

### Update Order Status

Endpoint:

```http
PUT /api/orders/{id}/status
```

Body:

```json
{
  "status": "Completed"
}
```

Expected result:

- Status code `204 No Content`.
- Order status is updated.

## 9. Admin Dashboard Summary

Use an admin token.

Endpoint:

```http
GET /api/admin/dashboard
```

Expected result:

- Status code `200 OK`.
- Returns totals for users, products, categories, orders, revenue, and pending orders.

## 10. Final Build Check

Run:

```powershell
dotnet build
```

Expected result:

```text
Build succeeded.
0 Warning(s)
0 Error(s)
```
