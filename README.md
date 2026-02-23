EStoreManagementAPI






ASP.NET Core 7 Web API for E-Store Management. Secure REST API with JWT authentication, SQLite database, and Swagger documentation.

Features

Full CRUD operations for products and categories

JWT Authentication for secure access (login + protected routes)

SQLite database with EF Core migrations

Swagger/OpenAPI UI for interactive API documentation

Docker support for containerized deployment

Input validation and descriptive error responses

Prerequisites

.NET 7 SDK

Docker (optional)

Getting Started
Local Development

Clone the repository

git clone https://github.com/Kristersssssss/EStoreManagementAPI.git
cd EStoreManagementAPI

Build the project

dotnet build

Apply database migrations

dotnet ef database update

Run the API

dotnet run --urls http://localhost:5000

Access endpoints:

Swagger UI: http://localhost:5000/swagger

API Root: http://localhost:5000

Health Check: http://localhost:5000/health

Docker Deployment

Build the Docker image:

docker build -t estoreapi .

Start with Docker Compose:

docker-compose up -d

Access endpoints:

Swagger UI: http://localhost:8080/swagger

API Root: http://localhost:8080

API Endpoints
Products
Method	Endpoint	Auth	Description
GET	/api/products	No	Get all products
GET	/api/products/{id}	No	Get product by ID
POST	/api/products	Yes	Create product
PUT	/api/products/{id}	Yes	Update product
DELETE	/api/products/{id}	Yes	Delete product
Categories
Method	Endpoint	Auth	Description
GET	/api/categories	No	Get all categories
GET	/api/categories/{id}	No	Get category by ID
POST	/api/categories	Yes	Create category
PUT	/api/categories/{id}	Yes	Update category
DELETE	/api/categories/{id}	Yes	Delete category
Orders (JWT Required)
Method	Endpoint	Description
GET	/api/orders	Get all orders
POST	/api/orders	Create an order
Authentication
Method	Endpoint	Description
POST	/api/auth/login	Authenticate user and issue JWT
Quick Start Example
1. Authenticate and get JWT Token
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"YourPassword"}'

Response Example:

{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}

Save the token for authorized requests.

2. Get All Products (No Auth Required)
curl http://localhost:5000/api/products

Response Example:

[
  { "id": 1, "name": "Laptop", "price": 1200, "categoryId": 2 },
  { "id": 2, "name": "Mouse", "price": 25, "categoryId": 3 }
]
3. Create a New Product (JWT Required)
curl -X POST http://localhost:5000/api/products \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{"name":"Keyboard","price":45,"categoryId":3}'
4. Update a Product (JWT Required)
curl -X PUT http://localhost:5000/api/products/3 \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{"name":"Mechanical Keyboard","price":55,"categoryId":3}'
5. Delete a Product (JWT Required)
curl -X DELETE http://localhost:5000/api/products/3 \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
Project Structure
├── Program.cs                  # App entry & endpoint config
├── EStoreManagementAPI.csproj  # Project file
├── Migrations/                 # EF Core migrations
├── shop.db                     # SQLite database (auto-generated)
├── Dockerfile                  # Container image definition
├── docker-compose.yml          # Docker compose file
└── README.md                   # (This file)
Database Schema

Users: Id, Email

Categories: Id, Name

Products: Id, Name, Price, CategoryId (FK)

Orders: Id, OrderDate, UserId (FK)

Recommendations

Role-based authorization (e.g., admin vs user)

Unit & integration tests

API rate limiting & logging

CI/CD workflow (GitHub Actions / Azure DevOps)

Postman collection or client examples

License

This project is open source. See LICENSE
 for details.
