# ArgenCash Backend API

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-316192?style=for-the-badge&logo=postgresql)
![Architecture](https://img.shields.io/badge/Architecture-Clean-success?style=for-the-badge)

## Why this project exists

Most personal finance apps assume one stable currency. ArgenCash is designed for volatile, dual-currency environments where users need to preserve historical value context, compare rate sources, and avoid distorted portfolio reporting.

## Tech stack

- .NET 10, ASP.NET Core Web API
- Entity Framework Core + PostgreSQL
- JWT authentication
- Clean Architecture (`Api`, `Application`, `Domain`, `Infrastructure`)

## Core capabilities

- User auth (`register`, `login`, `me`) with JWT
- Account management with exchange-rate profile support
- Transaction and transfer workflows for ledger tracking
- Category and budget endpoints for spending structure
- Live exchange rate endpoints (`/api/exchangerates/live`, `/api/exchangerates/live/batch`)

## Project structure

- `ArgenCash.Api`: HTTP layer, auth middleware, Swagger, DI wiring
- `ArgenCash.Application`: use cases, contracts, DTOs, service orchestration
- `ArgenCash.Domain`: business entities and invariants
- `ArgenCash.Infrastructure`: EF Core persistence, auth providers, external integrations

## Local setup

### Prerequisites

- .NET SDK 10+
- PostgreSQL 15+

### 1) Configure environment

At minimum, configure these settings (environment variables or `appsettings.Development.json`):

- `ARGENCASH_DB_CONNECTION`
- `ARGENCASH_JWT_ISSUER`, `ARGENCASH_JWT_AUDIENCE`, `ARGENCASH_JWT_SECRET`, `ARGENCASH_JWT_EXPIRATION_MINUTES`
- `ARGENCASH_VERIFICATION_TOKEN_SECRET`, `ARGENCASH_VERIFICATION_TOKEN_EXPIRATION_MINUTES`
- `ARGENCASH_EXCHANGE_RATE_API_BASE_URL`, `ARGENCASH_EXCHANGE_RATE_SOURCE_NAME`
- `ARGENCASH_ALLOWED_ORIGIN_0`

For EF design-time commands, you can override the database endpoint without changing runtime settings:

- `ARGENCASH_EF_DB_CONNECTION` (preferred for `dotnet ef` on local/non-pooler Postgres)

For email verification flows, also set SMTP settings:

- `ARGENCASH_SMTP_HOST`, `ARGENCASH_SMTP_PORT`, `ARGENCASH_SMTP_USERNAME`, `ARGENCASH_SMTP_PASSWORD`, `ARGENCASH_SMTP_FROM_NAME`, `ARGENCASH_SMTP_FROM_EMAIL`

### 2) Restore and run

```bash
dotnet restore "ArgenCash.slnx"
dotnet ef database update --project "ArgenCash.Infrastructure/ArgenCash.Infrastructure.csproj" --startup-project "ArgenCash.Api/ArgenCash.Api.csproj"
dotnet run --project "ArgenCash.Api/ArgenCash.Api.csproj"
```

PowerShell example using a local Postgres endpoint for EF only:

```powershell
$env:ARGENCASH_EF_DB_CONNECTION="Host=localhost;Port=5432;Database=ArgenCashDB;Username=postgres;Password=secret"
dotnet ef database update --project "ArgenCash.Infrastructure/ArgenCash.Infrastructure.csproj" --startup-project "ArgenCash.Api/ArgenCash.Api.csproj"
```

Default local API URL: `http://localhost:5018`

### 3) Open API docs

Swagger UI is available in development at:

- `http://localhost:5018/swagger`

## Developer commands

```bash
dotnet build "ArgenCash.slnx"
dotnet test
```

Note: there are currently no automated test projects in this repository, so `dotnet test` will return without running tests.

## Docker

The repository includes a production-oriented `Dockerfile`.

```bash
docker build -t argencash-backend .
docker run --rm -p 8080:8080 argencash-backend
```

## Security and public repo notes

- Do not commit real `.env` files, SMTP passwords, JWT secrets, or production connection strings.
- Rotate any local secrets used during development before going public.
- Prefer storing secrets in environment variables or a secret manager.

## License

This project is licensed under the MIT License. See `LICENSE`.
