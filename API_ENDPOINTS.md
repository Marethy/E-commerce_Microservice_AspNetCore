# API Endpoints Documentation
**Base URL (API Gateway):** `http://localhost:5000`

---

## ?? Product Service

### Products

#### GET `/api/products`
**Auth:** Bearer Token  
**Description:** Get all products with optional filtering

**Query Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| categoryId | guid | No | Filter by category ID |

**Sample Request:**
```http
GET http://localhost:5000/api/products?categoryId=3fa85f64-5717-4562-b3fc-2c963f66afa6
Authorization: Bearer <your_token>
```

**Sample Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": [
    {
  "id": "a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d",
      "no": "PRD-001",
      "name": "Nike Air Max 270",
   "description": "Comfortable running shoes",
      "price": 3200000,
      "stock": 50,
      "categoryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "categoryName": "Shoes",
      "brandId": "7b8c9d0e-1f2a-3b4c-5d6e-7f8a9b0c1d2e",
      "brandName": "Nike",
      "sellerId": "9c0d1e2f-3a4b-5c6d-7e8f-9a0b1c2d3e4f",
      "sellerName": "Nike Official Store",
      "averageRating": 4.5,
      "reviewCount": 128
    }
  ],
  "message": null
}
```

---

#### POST `/api/products`
**Auth:** Bearer Token + Admin Role  
**Description:** Create new product

**Request Body:**
```json
{
  "no": "string (required, max 50 chars)",
  "name": "string (required, max 250 chars)",
  "description": "string | null",
  "price": "decimal (required, > 0)",
  "stock": "int (required, >= 0)",
  "categoryId": "guid (required)",
  "brandId": "guid | null",
"sellerId": "guid | null"
}
```

**Sample Request:**
```http
POST http://localhost:5000/api/products
Authorization: Bearer <admin_token>
Content-Type: application/json

{
  "no": "PRD-999",
  "name": "Adidas Ultraboost 22",
  "description": "Premium running shoes with boost technology",
  "price": 4500000,
  "stock": 30,
  "categoryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "brandId": "2b3c4d5e-6f7a-8b9c-0d1e-2f3a4b5c6d7e",
  "sellerId": "4d5e6f7a-8b9c-0d1e-2f3a-4b5c6d7e8f9a"
}
```

**Sample Response (201 Created):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "f1e2d3c4-b5a6-7c8d-9e0f-1a2b3c4d5e6f",
    "no": "PRD-999",
    "name": "Adidas Ultraboost 22",
    "price": 4500000,
"stock": 30,
    "categoryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "brandId": "2b3c4d5e-6f7a-8b9c-0d1e-2f3a4b5c6d7e",
 "sellerId": "4d5e6f7a-8b9c-0d1e-2f3a-4b5c6d7e8f9a"
  },
  "message": null
}
```

**Error Cases:**
```json
// 400 Bad Request - Validation Error
{
  "isSuccess": false,
  "data": null,
  "message": "Price must be greater than 0"
}

// 409 Conflict - Duplicate Product No
{
  "isSuccess": false,
  "data": null,
  "message": "Product No 'PRD-999' already exists"
}

// 401 Unauthorized
{
  "isSuccess": false,
  "data": null,
  "message": "Unauthorized"
}
```

---

#### PUT `/api/products/{id}`
**Auth:** Bearer Token + Admin Role  
**Description:** Update existing product

**Request Body:**
```json
{
  "name": "string | null",
  "description": "string | null",
  "price": "decimal | null",
  "stock": "int | null",
  "categoryId": "guid | null",
  "brandId": "guid | null",
  "sellerId": "guid | null"
}
```

**Sample Request:**
```http
PUT http://localhost:5000/api/products/f1e2d3c4-b5a6-7c8d-9e0f-1a2b3c4d5e6f
Authorization: Bearer <admin_token>
Content-Type: application/json

{
  "price": 4200000,
  "stock": 25
}
```

**Sample Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "f1e2d3c4-b5a6-7c8d-9e0f-1a2b3c4d5e6f",
    "no": "PRD-999",
    "name": "Adidas Ultraboost 22",
    "price": 4200000,
    "stock": 25
  },
  "message": null
}
```

---

### Categories

| Method | Endpoint | Auth | Description | Response |
|--------|----------|------|-------------|----------|
| GET | `/api/categories` | Bearer | Get all categories (optional includeProducts query) | `ApiResult<List<CategoryDto>>` |
| GET | `/api/categories/{id}` | Bearer | Get category by ID | `ApiResult<CategoryDto>` |
| GET | `/api/categories/by-name/{name}` | Bearer | Get category by name | `ApiResult<CategoryDto>` |
| POST | `/api/categories` | Bearer | Create category | `ApiResult<CategoryDto>` |
| PUT | `/api/categories/{id}` | Bearer | Update category | `ApiResult<CategoryDto>` |
| DELETE | `/api/categories/{id}` | Bearer | Delete category | `204 NoContent` |

### Brands

| Method | Endpoint | Auth | Description | Response |
|--------|----------|------|-------------|----------|
| GET | `/api/brands` | Bearer | Get all brands | `ApiResult<List<BrandDto>>` |
| GET | `/api/brands/{id}` | Bearer | Get brand by ID | `ApiResult<BrandDto>` |
| GET | `/api/brands/by-slug/{slug}` | Bearer | Get brand by slug | `ApiResult<BrandDto>` |
| POST | `/api/brands` | Bearer | Create brand | `ApiResult<BrandDto>` |
| PUT | `/api/brands/{id}` | Bearer | Update brand | `ApiResult<BrandDto>` |
| DELETE | `/api/brands/{id}` | Bearer | Delete brand | `204 NoContent` |

### Sellers

| Method | Endpoint | Auth | Description | Response |
|--------|----------|------|-------------|----------|
| GET | `/api/sellers` | Bearer | Get all sellers (optional officialOnly query) | `ApiResult<List<SellerDto>>` |
| GET | `/api/sellers/{id}` | Bearer | Get seller by ID | `ApiResult<SellerDto>` |
| POST | `/api/sellers` | Bearer | Create seller | `ApiResult<SellerDto>` |
| PUT | `/api/sellers/{id}` | Bearer | Update seller | `ApiResult<SellerDto>` |
| DELETE | `/api/sellers/{id}` | Bearer | Delete seller | `204 NoContent` |

### Product Reviews

| Method | Endpoint | Auth | Description | Response |
|--------|----------|------|-------------|----------|
| GET | `/api/productreviews/product/{productId}` | Bearer | Get reviews by product | `ApiResult<List<ProductReviewDto>>` |
| GET | `/api/productreviews/user/{userId}` | Bearer | Get reviews by user | `ApiResult<List<ProductReviewDto>>` |
| GET | `/api/productreviews/{id}` | Bearer | Get review by ID | `ApiResult<ProductReviewDto>` |
| GET | `/api/productreviews/product/{productId}/statistics` | Bearer | Get product review statistics | `ApiResult<object>` |
| POST | `/api/productreviews` | Bearer | Create review | `ApiResult<ProductReviewDto>` |
| PUT | `/api/productreviews/{id}` | Bearer | Update review | `ApiResult<ProductReviewDto>` |
| DELETE | `/api/productreviews/{id}` | Bearer | Delete review | `204 NoContent` |
| POST | `/api/productreviews/{id}/helpful` | Bearer | Mark review as helpful | `ApiResult<object>` |

---

## ?? Customer Service

#### GET `/api/customers/{username}`
**Auth:** None  
**Description:** Get customer by username

**Sample Request:**
```http
GET http://localhost:5000/api/customers/john.doe
```

**Sample Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": 1,
    "username": "john.doe",
    "firstName": "John",
    "lastName": "Doe",
  "email": "john.doe@example.com",
    "phoneNumber": "+84901234567",
    "address": "123 Main St, HCMC"
  },
  "message": null
}
```

**Error Cases:**
```json
// 404 Not Found
{
  "isSuccess": false,
  "data": null,
  "message": "Customer with username 'john.doe' not found"
}
```

---

## ?? Basket Service

#### GET `/api/baskets/{username}`
**Auth:** None  
**Description:** Get basket by username

**Sample Request:**
```http
GET http://localhost:5000/api/baskets/john.doe
```

**Sample Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "username": "john.doe",
    "items": [
   {
        "itemNo": "PRD-001",
      "itemName": "Nike Air Max 270",
  "quantity": 2,
        "itemPrice": 3200000,
        "availableQuanlity": 50
      },
      {
        "itemNo": "PRD-002",
        "itemName": "Adidas Ultraboost",
        "quantity": 1,
        "itemPrice": 4500000,
        "availableQuanlity": 30
      }
    ],
    "totalPrice": 10900000
  },
  "message": null
}
```

---

#### POST `/api/baskets`
**Auth:** None  
**Description:** Update basket (add/update items)

**Request Body:**
```json
{
  "username": "string (required)",
  "items": [
    {
      "itemNo": "string (required)",
      "itemName": "string (required)",
 "quantity": "int (required, > 0)",
      "itemPrice": "decimal (required)"
    }
  ]
}
```

**Sample Request:**
```http
POST http://localhost:5000/api/baskets
Content-Type: application/json

{
  "username": "john.doe",
  "items": [
    {
      "itemNo": "PRD-001",
      "itemName": "Nike Air Max 270",
      "quantity": 3,
      "itemPrice": 3200000
    }
  ]
}
```

**Sample Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "username": "john.doe",
    "items": [
      {
        "itemNo": "PRD-001",
        "itemName": "Nike Air Max 270",
  "quantity": 3,
   "itemPrice": 3200000,
        "availableQuanlity": 50
      }
],
    "totalPrice": 9600000
  },
  "message": null
}
```

---

#### POST `/api/baskets/checkout`
**Auth:** None  
**Description:** Checkout and publish event to order service

**Request Body:**
```json
{
  "username": "string (required)",
  "firstName": "string (required)",
  "lastName": "string (required)",
  "emailAddress": "string (required, email format)",
  "shippingAddress": "string (required)",
  "invoiceAddress": "string | null"
}
```

**Sample Request:**
```http
POST http://localhost:5000/api/baskets/checkout
Content-Type: application/json

{
  "username": "john.doe",
  "firstName": "John",
  "lastName": "Doe",
  "emailAddress": "john.doe@example.com",
  "shippingAddress": "123 Main St, District 1, HCMC",
  "invoiceAddress": "123 Main St, District 1, HCMC"
}
```

**Sample Response (202 Accepted):**
```json
{
  "isSuccess": true,
  "data": null,
  "message": "Checkout event has been published"
}
```

**Error Cases:**
```json
// 404 Not Found - Basket not found
{
  "isSuccess": false,
  "data": null,
  "message": "Basket not found"
}

// 400 Bad Request - Validation error
{
  "isSuccess": false,
  "data": null,
  "message": "Invalid checkout data"
}
```

---

#### GET `/api/baskets/{username}/validate`
**Auth:** None  
**Description:** Validate cart before checkout (check stock availability)

**Sample Response (200 OK - Valid):**
```json
{
  "isSuccess": true,
  "data": {
    "isValid": true,
    "message": "Cart is valid",
    "issues": [],
    "invalidItems": [],
    "totalItems": 2,
    "totalPrice": 10900000
  },
  "message": null
}
```

**Sample Response (200 OK - Invalid):**
```json
{
  "isSuccess": true,
  "data": {
    "isValid": false,
    "message": "Cart has issues",
    "issues": [
      "Only 5 of Nike Air Max 270 available (requested: 10)",
      "Adidas Ultraboost is out of stock"
    ],
  "invalidItems": [
      {
        "itemNo": "PRD-001",
     "itemName": "Nike Air Max 270",
        "issue": "INSUFFICIENT_STOCK",
        "requested": 10,
        "available": 5
      },
      {
        "itemNo": "PRD-002",
   "itemName": "Adidas Ultraboost",
      "issue": "OUT_OF_STOCK"
      }
    ],
    "totalItems": 2,
    "totalPrice": 10900000
  },
  "message": null
}
```

---

## ?? Order Service

#### GET `/api/v1/orders/users/{userName}`
**Auth:** None  
**Description:** Get all orders by username

**Sample Request:**
```http
GET http://localhost:5000/api/v1/orders/users/john.doe
```

**Sample Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": [
    {
   "id": 1,
      "userName": "john.doe",
      "totalPrice": 10900000,
      "firstName": "John",
      "lastName": "Doe",
      "emailAddress": "john.doe@example.com",
      "shippingAddress": "123 Main St, District 1, HCMC",
      "invoiceAddress": "123 Main St, District 1, HCMC",
      "status": 1,
      "createdDate": "2024-01-15T10:30:00Z",
      "lastModifiedDate": "2024-01-15T10:30:00Z"
    }
  ],
  "message": null
}
```

---

#### POST `/api/v1/orders`
**Auth:** None  
**Description:** Create new order

**Request Body:**
```json
{
  "userName": "string (required)",
  "totalPrice": "decimal (required, > 0)",
  "firstName": "string (required)",
  "lastName": "string (required)",
  "emailAddress": "string (required, email format)",
  "shippingAddress": "string (required)",
  "invoiceAddress": "string | null"
}
```

**Sample Request:**
```http
POST http://localhost:5000/api/v1/orders
Content-Type: application/json

{
  "userName": "john.doe",
  "totalPrice": 10900000,
  "firstName": "John",
  "lastName": "Doe",
  "emailAddress": "john.doe@example.com",
  "shippingAddress": "123 Main St, District 1, HCMC",
  "invoiceAddress": "123 Main St, District 1, HCMC"
}
```

**Sample Response (201 Created):**
```json
{
  "isSuccess": true,
  "data": 1,
  "message": null
}
```

**Error Cases:**
```json
// 400 Bad Request
{
  "isSuccess": false,
  "data": null,
  "message": "Invalid order data"
}
```

---

#### GET `/api/v1/orders/{id}/invoice`
**Auth:** None  
**Description:** Download order invoice PDF

**Sample Request:**
```http
GET http://localhost:5000/api/v1/orders/1/invoice
```

**Response:** Binary PDF file  
**Content-Type:** `application/pdf`  
**Filename:** `Invoice-1-20240115.pdf`

---

## ?? Inventory Service

#### GET `/api/inventory/items/{itemNo}`
**Auth:** None  
**Description:** Get all inventory entries by item number

**Sample Request:**
```http
GET http://localhost:5000/api/inventory/items/PRD-001
```

**Sample Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": [
    {
    "id": "507f1f77bcf86cd799439011",
      "itemNo": "PRD-001",
      "quantity": 50,
      "documentType": "Purchase",
      "documentNo": "PUR-20240115-001",
      "createdDate": "2024-01-15T08:00:00Z"
    }
  ],
  "message": null
}
```

---

#### POST `/api/inventory/purchase/{itemNo}`
**Auth:** None  
**Description:** Purchase item into inventory

**Request Body:**
```json
{
  "quantity": "int (required, > 0)",
  "documentNo": "string | null"
}
```

**Sample Request:**
```http
POST http://localhost:5000/api/inventory/purchase/PRD-001
Content-Type: application/json

{
  "quantity": 100,
  "documentNo": "PUR-20240115-002"
}
```

**Sample Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "507f1f77bcf86cd799439012",
    "itemNo": "PRD-001",
    "quantity": 100,
  "documentType": "Purchase",
    "documentNo": "PUR-20240115-002",
    "createdDate": "2024-01-15T09:00:00Z"
  },
  "message": null
}
```

---

## ?? Chatbot Service

#### POST `/api/chat`
**Auth:** None  
**Description:** Send chat message to AI chatbot

**Request Body:**
```json
{
  "message": "string (required)",
  "sessionId": "string | null"
}
```

**Sample Request:**
```http
POST http://localhost:5000/api/chat
Content-Type: application/json

{
  "message": "What are the best running shoes?",
  "sessionId": "user-session-123"
}
```

**Sample Response (200 OK):**
```json
{
  "response": "Based on our catalog, here are the top running shoes...",
  "sessionId": "user-session-123"
}
```

**Rate Limit:** 60 requests per minute  
**Timeout:** 30 seconds

---

## ?? Identity Service (Not via Gateway - Direct Access)

### Token Management

#### GET `/api/token`
**Auth:** None  
**Description:** Get authentication token (for testing purposes)

**Sample Request:**
```http
GET http://localhost:5000/api/token
```

**Sample Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 3600
}
```

---

### Users Management

#### POST `/api/users`
**Auth:** Bearer Token  
**Description:** Create new user with role

**Request Body:**
```json
{
  "username": "string (required)",
  "email": "string (required, email format)",
  "password": "string (required, min 6 chars)",
  "firstName": "string | null",
  "lastName": "string | null",
  "role": "string (required: 'Administrator' | 'Customer' | 'Seller')"
}
```

**Sample Request:**
```http
POST http://localhost:5000/api/users
Authorization: Bearer <admin_token>
Content-Type: application/json

{
  "username": "jane.smith",
  "email": "jane.smith@example.com",
  "password": "SecurePass123!",
  "firstName": "Jane",
  "lastName": "Smith",
  "role": "Customer"
}
```

**Sample Response (201 Created):**
```json
{
  "id": "a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d",
  "userName": "jane.smith",
  "email": "jane.smith@example.com",
  "firstName": "Jane",
  "lastName": "Smith",
  "roles": ["Customer"]
}
```

---

## ?? Common Response Structure

### Success Response
```json
{
  "isSuccess": true,
  "data": { /* actual data or null */ },
  "message": null
}
```

### Error Response
```json
{
  "isSuccess": false,
  "data": null,
  "message": "Error message describing what went wrong"
}
```

---

## ?? Authentication

- **Bearer Token:** Required for protected endpoints
- **Admin Role:** Required for admin-only operations (Product CRUD, User Management)
- **Claim Requirements:** Some endpoints require specific function/command claims

### Getting a Token
```http
GET http://localhost:5000/api/token
```

---

## ?? API Gateway Configuration

- **Rate Limiting:** Enabled on Product (30 req/10s) & Chatbot (60 req/min) services
- **Circuit Breaker:** QoS enabled with 2 exceptions before breaking
- **Caching:** 15-second cache for Product GET endpoints
- **Timeout:** 5 seconds for Product, 30 seconds for Chatbot
- **Load Balancing:** Ready for multiple downstream instances

---

## ?? Frontend Integration Notes

### 1. **Standard Headers**
```javascript
{
  'Content-Type': 'application/json',
  'Authorization': 'Bearer ' + token,
  'X-Correlation-Id': generateUUID() // optional for tracking
}
```

### 2. **Error Handling Pattern**
```javascript
try {
  const response = await fetch(url, options);
const result = await response.json();
  
  if (result.isSuccess) {
    return result.data;
  } else {
    throw new Error(result.message);
  }
} catch (error) {
  console.error('API Error:', error.message);
}
```

### 3. **Common Data Types**
- **GUID Format:** `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx` (lowercase with hyphens)
- **DateTime Format:** ISO 8601 UTC (`2024-01-15T10:30:00Z`)
- **Decimal/Price:** Number (e.g., `3200000` not string)
- **Response Wrapper:** Always check `isSuccess` before accessing `data`

### 4. **Important Business Rules**
- **Price:** Must be > 0
- **Quantity/Stock:** Must be >= 0
- **Email:** Must be valid email format
- **Product No:** Must be unique across system
- **Basket Checkout:** Automatically validates stock before publishing event
- **Order Status:** 0=Pending, 1=Confirmed, 2=Shipped, 3=Delivered, 4=Cancelled

---

## ?? Quick Start Examples

### Login & Get Products
```javascript
// 1. Get token
const tokenResponse = await fetch('http://localhost:5000/api/token');
const { token } = await tokenResponse.json();

// 2. Get products
const productsResponse = await fetch('http://localhost:5000/api/products', {
  headers: { 'Authorization': `Bearer ${token}` }
});
const result = await productsResponse.json();

if (result.isSuccess) {
  console.log('Products:', result.data);
}
```

### Add to Basket & Checkout
```javascript
// 1. Add to basket
await fetch('http://localhost:5000/api/baskets', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    username: 'john.doe',
    items: [
      { itemNo: 'PRD-001', itemName: 'Nike Air Max', quantity: 2, itemPrice: 3200000 }
    ]
  })
});

// 2. Validate basket
const validation = await fetch('http://localhost:5000/api/baskets/john.doe/validate');
const validationResult = await validation.json();

if (validationResult.data.isValid) {
  // 3. Checkout
  await fetch('http://localhost:5000/api/baskets/checkout', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      username: 'john.doe',
      firstName: 'John',
      lastName: 'Doe',
emailAddress: 'john.doe@example.com',
      shippingAddress: '123 Main St, HCMC'
  })
  });
}
```

---

## ?? Important Notes

1. All responses follow standardized `ApiResult<T>` pattern
2. Frontend should **only communicate with API Gateway (port 5000)**
3. Identity & Saga services are accessed directly (not via gateway)
4. Timestamps are in **UTC format** (ISO 8601)
5. GUIDs use standard format (lowercase with hyphens)
6. Always validate basket before checkout to prevent stock issues
7. Rate limits are enforced - implement retry logic with exponential backoff
8. Binary responses (PDFs) don't use ApiResult wrapper
