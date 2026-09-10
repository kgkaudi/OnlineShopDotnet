# **OnlineShopDotnet**

An extensible **e‑commerce backend** built with **ASP.NET Core**, **MongoDB**, and **JWT authentication**.  
It provides a clean architecture with repositories, services, DTOs, controllers, and seed data for admin users and initial collections.

---

## **Features**

### **Authentication & Authorization**
- User registration & login  
- BCrypt password hashing  
- JWT token generation  
- Role‑based authorization (`Admin`, `User`)

### **Domain Modules**
- Users  
- Products  
- Categories  
- Inventory events  
- Cart  
- Wishlist  
- Coupons  
- Orders  

### **Database**
- MongoDB  
- Repository pattern  
- Automatic admin seeding  
- Clean collection structure

### **API**
- RESTful endpoints  
- Swagger UI enabled  
- Postman collection included  
- Consistent route structure:  
  ```
  /api/<resource>
  ```

---

## **Tech Stack**

| Component | Technology |
|----------|------------|
| Backend | ASP.NET Core 8 |
| Database | MongoDB |
| Auth | JWT + BCrypt |
| DI | Built‑in ASP.NET Core DI |
| Docs | Swagger |
| Testing | Postman collection |

---

## **Project Structure**

```
backend/
└── OnlineShop.Api/
    ├── Controllers/
    ├── Services/
    ├── Repositories/
    ├── Models/
    ├── Dtos/
    ├── Settings/
    ├── Program.cs
    └── appsettings.json
```

---

## **Setup Instructions**

### **1. Install dependencies**
- .NET SDK 8+
- MongoDB (local or Docker)

### **2. Configure MongoDB**
Edit `appsettings.json`:

```json
"MongoDB": {
  "ConnectionString": "mongodb://localhost:27017",
  "DatabaseName": "OnlineShopDb"
}
```

### **3. Run the API**

```bash
cd backend/OnlineShop.Api
dotnet run
```

API will start at:

```
http://localhost:5000
```

Swagger UI:

```
http://localhost:5000/swagger
```

---

## **Admin User**

On first run, the system seeds an admin user:

```
Email: admin@shop.com
Password: admin
Role: Admin
```

Password is stored using BCrypt.

---

## **Postman Collection**

A full Postman collection is included with all routes using:

```
{{baseUrl}}/api/
```

Set environment variables:

- `baseUrl = http://localhost:5000`
- `token = <JWT after login>`
- `productId`
- `categoryId`

---

## **API Overview**

### **Auth**
- `POST /api/auth/register`
- `POST /api/auth/login`

### **Users**
- `GET /api/users` (Admin)

### **Products**
- `GET /api/products`
- `POST /api/products` (Admin)

### **Categories**
- `GET /api/categories`

### **Inventory**
- `POST /api/inventory` (Admin)

### **Cart**
- `GET /api/cart`
- `POST /api/cart/add`

### **Wishlist**
- `GET /api/wishlist`
- `POST /api/wishlist/add`

### **Coupons**
- `GET /api/coupons`
- `POST /api/coupons` (Admin)

### **Orders**
- `GET /api/orders`

---

## **Development Notes**

- All repositories use `MongoDB:ConnectionString` and `MongoDB:DatabaseName`.
- Passwords must always be hashed using BCrypt.
- JWT expiration is configurable via `IJwtService`.
- Admin seeding runs only when the `Users` collection is empty.

---

## **License**

MIT License.