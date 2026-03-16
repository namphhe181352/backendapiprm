# Restaurant Backend API Endpoints

## Auth
- POST `/api/auth/login`
- POST `/api/auth/register`
- POST `/api/auth/change-password`
- GET `/api/auth/me`

## Areas
- GET `/api/areas`
- GET `/api/areas/{id}`
- POST `/api/areas`
- PUT `/api/areas/{id}`
- PATCH `/api/areas/{id}/active`

## Tables
- GET `/api/tables?areaId=&status=&keyword=&page=&pageSize=`
- GET `/api/tables/{id}`
- POST `/api/tables`
- PUT `/api/tables/{id}`
- PATCH `/api/tables/{id}/active`
- PATCH `/api/tables/{id}/status`

## Categories
- GET `/api/categories`
- GET `/api/categories/{id}`
- POST `/api/categories`
- PUT `/api/categories/{id}`
- PATCH `/api/categories/{id}/active`

## Menu Items
- GET `/api/menu-items?categoryId=&isAvailable=&keyword=&page=&pageSize=`
- GET `/api/menu-items/{id}`
- POST `/api/menu-items`
- PUT `/api/menu-items/{id}`
- PATCH `/api/menu-items/{id}/availability`

## Reservations
- GET `/api/reservations?status=&tableId=&staffId=&from=&to=&page=&pageSize=`
- GET `/api/reservations/{id}`
- POST `/api/reservations/check-in`
- PUT `/api/reservations/{id}`
- PATCH `/api/reservations/{id}/cancel`

## Orders
- GET `/api/orders/{id}`
- GET `/api/orders/by-reservation/{reservationId}`
- GET `/api/orders/{orderId}/items`
- POST `/api/orders/{orderId}/items`
- PATCH `/api/order-items/{orderDetailId}/status`

## Checkout / Invoices
- POST `/api/checkout`
- GET `/api/invoices/{id}`
- GET `/api/invoices?from=&to=&staffId=&paymentMethod=&page=&pageSize=`

## Admin (admin only)
- GET `/api/admin/dashboard/summary`
- GET `/api/admin/statistics/revenue?period=today|week|month`
- GET `/api/admin/statistics/top-items?from=&to=&limit=`
- GET `/api/admin/staff?keyword=&isActive=&page=&pageSize=`
- GET `/api/admin/staff/{id}`
- POST `/api/admin/staff`
- PUT `/api/admin/staff/{id}`
- PATCH `/api/admin/staff/{id}/active`

## Notifications / Settings
- GET `/api/notifications`
- PATCH `/api/notifications/{id}/read`
- GET `/api/settings/me`
- PUT `/api/settings/me`

## Main flow
1. Login
2. Check-in reservation (`/api/reservations/check-in`)
3. Add items (`/api/orders/{orderId}/items`)
4. Update kitchen item status (`/api/order-items/{orderDetailId}/status`)
5. Checkout (`/api/checkout`)

## Local run
1. Update `appsettings.json` connection string and `Jwt` values.
2. Run `dotnet build backendapi.sln`
3. Run `dotnet run --project backendapi.csproj`
4. Open `/swagger` and authorize with Bearer token.

## Current limitations
- Notification module is temporary in-memory.
- Settings module updates current user profile only.
- Analytics are query-based and not yet optimized by materialized/report tables.
