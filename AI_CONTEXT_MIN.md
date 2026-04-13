# ArgenCash Backend - Quick AI Context

- Stack: .NET 10 + ASP.NET Core + EF Core + PostgreSQL + JWT
- Layers: `Api`, `Application`, `Domain`, `Infrastructure`
- Branch: `develop`
- Remote: `https://github.com/giuliRicca/ArgenCash.git`

## Run
- Migrate:
  - `dotnet ef database update --project "ArgenCash.Infrastructure/ArgenCash.Infrastructure.csproj" --startup-project "ArgenCash.Api/ArgenCash.Api.csproj"`
- Start:
  - `dotnet run --project "ArgenCash.Api/ArgenCash.Api.csproj"`

## Required Config
- `ConnectionStrings:DefaultConnection`
- `Jwt:*`
- `ExchangeRateApi:*`
- `AllowedOrigins`

## Domain Highlights
- Account has `ExchangeRateType`
- Transfer = 2 linked transactions via `TransferGroupId`
- Deleting transfer leg deletes full transfer group
- Live rates support `/live` and `/live/batch` with provider cache

## Key APIs
- `/api/auth/*`
- `/api/accounts*`
- `/api/transactions*`
- `/api/transfers`
- `/api/categories*`
- `/api/exchangerates/*`
