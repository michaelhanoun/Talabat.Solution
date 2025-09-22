# Talabat.Solution (ASP.NET Core 8)

A production-style backend for a Talabat-like food delivery system. It showcases clean architecture, JWT auth + Google OAuth, Redis-backed caching, Stripe payments, SMTP email, and a specification-driven data access layer.

## 🚀 Highlights

- **.NET 8 Web API** with minimal `Program.cs`, feature-oriented controllers, and Swagger.
- **Authentication & Identity**
  - Email/password with **JWT** issuance.
  - **Google OAuth** external login flow (`/api/Account/external-login`, callback supported).
  - User management via **ASP.NET Core Identity** .
- **Payments**: **Stripe** PaymentIntents + webhook to update order status.
- **Caching**
  - **Response caching** attribute (`[Cached(seconds)]`) backed by **Redis**.
  - Custom cache key generation from URL + sorted query.
- **Data & Patterns**
  - **EF Core (SQL Server)** with migrations & **data seeding** (brands, categories, products, delivery methods).
  - **Specification Pattern**, **Generic Repository**, **Unit of Work**.
  - **AutoMapper** DTO mapping + URL resolvers for images.
- **Reliability**
  - Central **ExceptionMiddleware** with consistent API error shapes.
  - Validation responses via `ApiValidationErrorResponse`.
- **Emails**: SMTP service for password reset / notifications.

---

## 🧱 Solution Structure

```text
Talabat.Solution/
 ├─ Talabat.APIs/   # API layer (controllers, DTOs, middleware, Swagger)
 │  ├─ Controllers/
 │  │   ├─ AccountController.cs   # Register, Login, External Login, Current User, Address, Forgot/Reset Password
 │  │   ├─ ProductsController.cs  # Products, Brands, Categories (+ [Cached])
 │  │   ├─ BasketController.cs    # Basket CRUD (Redis)
 │  │   ├─ OrdersController.cs    # Create/Get orders, delivery methods (JWT [Authorize])
 │  │   └─ PaymentController.cs   # Stripe PaymentIntent + webhook
 │  ├─ Dtos/
 │  ├─ Extensions/    # Add*Services, Swagger, UserManager helpers
 │  ├─ Helpers/       # MappingProfile, CachedAttribute, Url resolvers
 │  ├─ Middlewares/   # ExceptionMiddleware
 │  └─ wwwroot/images/products/   # Static product images
 │
 ├─ Talabat.Core/     # Domain & contracts
 │  ├─ Entities/
 │  │   ├─ Product (Brand/Category/Product)
 │  │   ├─ Basket (CustomerBasket, BasketItem)
 │  │   ├─ Order_Aggregate (Order, OrderItem, DeliveryMethod, Address, OrderStatus)
 │  │   └─ Identity (ApplicationUser, Address)
 │  ├─ Specifications/     # Base + Product & Order specs, params & pagination
 │  └─ Services.Contract/  # IProductService, IOrderService, IPaymentService, IAuthService, IResponseCacheService, ISendEmail, etc.
 │
 ├─ Talabat.Repository/    # Infrastructure: EF Core, Migrations, Seeding
 │  ├─ _Data/              # StoreContext, configurations, seeding (brands/categories/products/delivery)
 │  ├─ _Identity/          # Identity DbContext + user seed
 │  ├─ Generic Repository/ # GenericRepository, SpecificationsEvaluator
 │  ├─ Basket Repository/  # IBasketRepository → Redis
 │  └─ UnitOfWork.cs
 │
 └─ Talabat.Service/       # Application services
    ├─ Product Service/    # Filtering, sorting, paging via specifications
    ├─ Order Service/      # Create order from basket, delivery, totals
    ├─ Payment Service/    # Stripe PaymentIntent, webhook status updates
    ├─ AuthService/        # JWT token factory
    └─ Cache Service/      # Redis-backed response cache


---

## 🗝️ Key Endpoints (examples)

- **Auth / Account**
  - `POST /api/Account/register`
  - `POST /api/Account/login`
  - `GET  /api/Account/currentUser` (JWT)
  - `GET  /api/Account/external-login?provider=Google`
  - `GET  /api/Account/external-login-callback`
  - `POST /api/Account/forgetPassword`, `POST /api/Account/resetPassword`
  - `GET/PUT /api/Account/address` (JWT)
- **Products**
  - `GET /api/Products` — supports `pageIndex`, `pageSize`, `sort`, `brandId`, `categoryId`, `search`
  - `GET /api/Products/brands`  (cached)
  - `GET /api/Products/categories` (cached)
  - `GET /api/Products/{id}`
- **Basket (Redis)**
  - `GET /api/Basket?id={basketId}`
  - `POST /api/Basket` (create/update)
  - `DELETE /api/Basket?id={basketId}`
- **Orders (JWT)**
  - `POST /api/Orders` — create order from basket + delivery method + shipping address
  - `GET  /api/Orders`
  - `GET  /api/Orders/{id}`
  - `GET  /api/Orders/deliveryMethods`
- **Payments (Stripe)**
  - `POST /api/Payment/{basketId}` — create/update PaymentIntent
  - `POST /api/Payment/webhook` — handle Stripe events

---

## 🧪 Tech Stack

- **ASP.NET Core 8**, **EF Core (SQL Server)**, **Identity**
- **JWT** + **Google OAuth**
- **Stripe** SDK
- **Redis** (StackExchange.Redis)
- **AutoMapper**
- **Swagger (Swashbuckle)**


---

## 👨‍💻 Author

**Michael Hanoun**  
- 📧 Email: michelhanoun210@gmail.com  
- 🔗 [LinkedIn](https://www.linkedin.com/in/michael-hanoun)  
- 🔗 [GitHub](https://github.com/michaelhanoun)  

---
