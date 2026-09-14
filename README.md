# LoginService

A simple ASP.NET Core login service project.

## Tech Stack

- ASP.NET Core
- Entity Framework Core
- SQL Server

## Prerequisites

- .NET SDK 10+
- SQL Server instance or container

## Run locally

```bash
dotnet restore
dotnet run
```

## Project structure

- `Controllers` - API endpoints
- `Services` - business logic
- `Repositories` - database access
- `Models` - entities and DTOs
- `Data` - EF Core DbContext

## Configuration
This project uses **.NET User Secrets** for sensitive configuration values such as the JWT signing key and SQL Server connection string.

### 1. Initialize User Secrets
From the project directory:

```bash
dotnet user-secrets init
```

### 2. Configure JWT Secret
Set the JWT signing key:

```bash
dotnet user-secrets set "Jwt:Key" "your-long-random-secret-key"
```

### 3. Verify Secrets
You can verify that the secrets have been configured:

```bash
dotnet user-secrets list
```

## Database Setup
This project uses **SQL Server for local development**. SQL Server is run locally using the official Microsoft SQL Server Docker image.

The repository does **not** contain the SQL Server image or database. Each developer runs their own local SQL Server instance.

### 1. Start SQL Server locally
Make sure Docker Desktop is running and execute:

```bash
docker run -e "ACCEPT_EULA=Y" \
  -e "MSSQL_SA_PASSWORD=YourStrongPassword123!" \
  -p 1433:1433 \
  --name auth-sqlserver \
  -d mcr.microsoft.com/mssql/server:2025-latest
```
Verify the container is running:

```bash
docker ps
```

### 2. Configure the connection string
Configure the database connection using .NET User Secrets:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=AuthDb;User Id=sa;Password=YourStrongPassword123!;TrustServerCertificate=True;"
```
Replace the password with the password used when creating the SQL Server container.

### 3. Create the database
Apply the EF Core migrations:

```bash
dotnet ef database update
```
This will create the `AuthDb` database and the required tables.

### Database Architecture

```text
ASP.NET Core Web API
        │
        ▼
    EF Core
        │
        ▼
localhost:1433
        │
        ▼
SQL Server (Docker)
        │
        ▼
     AuthDb
```

> The Docker container is used only to provide a local SQL Server instance. The application itself is not containerized or deployed using Docker.

## Swagger

After running the app, open:

- `https://localhost:7183/swagger`
- or `http://localhost:5299/swagger`

## Notes

This project is a starter backend for login/authentication functionality and can be extended with JWT, password hashing, and user registration flows.
