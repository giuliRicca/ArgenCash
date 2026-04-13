# AI Context - ArgenCash Backend

## Project Snapshot
- Name: ArgenCash backend API
- Stack: .NET 10, ASP.NET Core, EF Core, PostgreSQL, JWT auth
- Architecture: Clean Architecture (`Api`, `Application`, `Domain`, `Infrastructure`)

## Repository
- Remote: `https://github.com/giuliRicca/ArgenCash.git`
- Main working branch: `develop`
- Solution file: `ArgenCash.slnx`

## Local Setup
1. Configure DB and auth in `ArgenCash.Api/appsettings.Development.json`.
2. Apply migrations:
   - `dotnet ef database update --project "ArgenCash.Infrastructure/ArgenCash.Infrastructure.csproj" --startup-project "ArgenCash.Api/ArgenCash.Api.csproj"`
3. Run API:
   - `dotnet run --project "ArgenCash.Api/ArgenCash.Api.csproj"`

## Required Configuration
- `ConnectionStrings:DefaultConnection`
- `Jwt:Issuer`
- `Jwt:Audience`
- `Jwt:SecretKey`
- `Jwt:ExpirationMinutes`
- `ExchangeRateApi:BaseUrl`
- `ExchangeRateApi:SourceName`
- `AllowedOrigins`

## Core Domain Notes
- Accounts support preferred `ExchangeRateType`.
- Transactions support optional transfer fields:
  - `TransferGroupId`
  - `CounterpartyAccountId`
- Transfers create linked expense/income transaction pairs.
- Deleting one transfer leg deletes the full transfer group.
- Exchange-rate endpoints support single and batch live reads.
- Live rates are cached in-memory in infrastructure provider.

## Important Endpoints
- Auth: `/api/auth/register`, `/api/auth/login`, `/api/auth/me`
- Accounts: `/api/accounts`, `/api/accounts/{id}`
- Transactions: `/api/transactions`, `/api/transactions/{id}`
- Transfers: `/api/transfers`
- Categories: `/api/categories`
- Exchange rates:
  - `/api/exchangerates/live`
  - `/api/exchangerates/live/batch`
  - `/api/exchangerates/latest`
  - `/api/exchangerates/manual`

## Database / Migrations
- Current migration chain includes:
  - `20260407170411_Initial`
  - `20260408192050_AddExchangeRateTypeToAccountAndRateLookupIndex`
  - `20260410011526_AddTransactionTransferFields`

## Conventions For AI Edits
- Keep enum usage centralized; avoid duplicated string literals.
- For new response fields, update both DTO and repository projections.
- Keep ownership checks user-scoped in repository/service operations.
- Prefer additive, non-breaking API changes.

## Deployment Notes
- Do not deploy with local secrets from development config.
- Ensure CORS origins include full URLs with scheme.
- Run DB migrations before serving traffic.
