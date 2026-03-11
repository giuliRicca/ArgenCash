# 🌐 ArgenCash Ledger API 

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-316192?style=for-the-badge&logo=postgresql)
![Architecture](https://img.shields.io/badge/Architecture-Clean-success?style=for-the-badge)

A high-performance, multi-currency financial ledger designed specifically for dual-currency economies and volatile exchange markets.

## 📖 The Business Problem
Standard personal finance applications fail in high-inflation or dual-currency environments (like Argentina) because they assume a stable, single-rate exchange system. Users earning in a hard currency (USD) but transacting in a local currency (ARS) face "hidden inflation" and require a system that understands the gap between official and parallel market rates.

ArgenCash solves this by implementing a **Forex-Aware Double-Entry Ledger**. It tracks the exact historical exchange rate at the microsecond a transaction occurs, ensuring that a user's true net worth is never skewed by future currency devaluations.

## 🚀 Core Fintech Features
* **Multi-Rate Triangulation:** Supports concurrent exchange rates for a single currency pair (e.g., Official, Blue, and MEP rates).
* **Snapshot Pattern:** Transactions are immutable. Historical ARS expenses retain their exact USD value from the day they were executed, preventing historical data mutation.
* **Financial Precision:** 100% eradication of floating-point truncation errors by utilizing `numeric(19,4)` and the Money Pattern across the entire stack.
* **Idempotency:** Core transaction endpoints utilize Idempotency Keys to safely handle network drops and prevent double-charging from frontend retries.
* **Automated Data Sync:** Implements background workers to fetch market rates periodically without blocking the main transaction threads.

## 🏗️ Architecture
This API is built using **Clean Architecture** (Domain-Driven Design principles) to enforce a strict separation of concerns, ensuring the financial domain logic is entirely isolated from web frameworks and database providers.

* **Presentation:** ASP.NET Core Web API
* **Application:** CQRS pattern, FluentValidation
* **Domain:** Rich enterprise entities, Custom Exceptions
* **Infrastructure:** Entity Framework Core, PostgreSQL, external API integrations (DolarApi)

## 🛠️ Getting Started

### Prerequisites
* .NET 8 SDK or later
* PostgreSQL 15+
* Docker (Optional, for database containerization)

### Local Setup
1. Clone the repository:
   ```bash
   git clone [https://github.com/giuliRicca/ArgenCash.git](https://github.com/giuliRicca/ArgenCash.git)