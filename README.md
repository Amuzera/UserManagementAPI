# UserManagementAPI
A .NET 9 User Management API exposing Swagger-documented CRUD endpoints with JWT auth, input validation, centralized exception handling (Problem Details), and request/response logging

## Description

A .NET 9 Minimal API for managing users.
Built as part of a back-end development activity, with the help of Microsoft Copilot to scaffold, enhance, debug, and polish the codebase.

## Tech Stack

- .NET 9

- Minimal APIs

- EF Core (InMemory)

- Swagger / OpenAPI

- JWT Bearer Authentication

- Custom Middleware (logging, error handling)

- Problem Details for validation & error responses

## Features

- CRUD operations for users (/api/users)

- Input validation with DataAnnotations + custom regex

- Pagination & search in GET /api/users

- JWT authentication (with /auth/token issuing dev tokens)

- Global exception handling returning consistent JSON

- Request/response logging middleware

- Health check endpoint (/health)

## Security

- All /api/users/* endpoints require a valid Bearer token

- Authorization: Bearer <token> header

- Tokens issued from /auth/token

## Testing

- Swagger UI: interactive docs + “Authorize” button for JWT

- Postman/cURL examples:

- POST /auth/token → get a token

- GET /api/users → list users (requires token)

- POST /api/users → create user

- PUT /api/users/{id} → update

- DELETE /api/users/{id} → delete

- Invalid inputs → 400 Bad Request with Problem Details

- Missing/invalid token → 401 Unauthorized

- Unknown ID → 404 Not Found

## Copilot’s Contributions

- Copilot assisted in several stages of development:

- Scaffolding

- Generated Program.cs boilerplate

- Suggested folder structure (Dtos, Middleware, Models…)

- CRUD Endpoints

- Generated base handlers for GET, POST, PUT, DELETE

- Helped implement DTOs and example schema filters for Swagger

- Debugging

- Pointed out missing validation → added ValidationHelper

- Suggested 404 handling for missing IDs

- Added CancellationToken to EF Core calls for best practice

- Middleware

- Helped draft logging middleware (request/response + elapsed ms)

- Generated a starting point for global exception handling

- Suggested middleware pipeline order: Error → Auth → Logging

- Enhancements

- JWT authentication setup (token validation + /auth/token)

- Global exception handler returning Problem Details

- Pagination logic (PagedResult<T>)

## How to Run Locally
### Clone
git clone https://github.com/<your-username>/UserManagementAPI.git
cd UserManagementAPI

### Restore packages
dotnet restore

### Run
dotnet run


## Navigate to:
https://localhost:XXXX/swagger

## Project Structure
UserManagementAPI/
 - Common/               # helpers (validation, filters)
 - Data/                 # EF Core DbContext
 - Dtos/                 # Request/Response DTOs
 - Middleware/           # Custom middleware (logging, errors)
 - Models/               # Entities
 - Swagger/              # Swagger filters
 - Program.cs            # Entry point
 - appsettings.json      # Config
 - UserManagementAPI.csproj
 ┗ README.md
