# Trading Journal — Backend

> ASP.NET Core 10 API for an AI-powered options trading journal. Built with Clean Architecture and CQRS.

![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql&logoColor=white)
![EF Core](https://img.shields.io/badge/EF_Core-10-512BD4)
![Auth0](https://img.shields.io/badge/Auth0-EB5424?logo=auth0&logoColor=white)
![Claude](https://img.shields.io/badge/Claude-Sonnet_4.5-D97706)

## ✨ Features

- 🔐 **JWT auth** via Auth0 — every endpoint protected, user-scoped data
- 📝 **Trades CRUD** — with chart + IBKR screenshot file upload
- 📊 **Dashboard aggregation** — P&L, win rate, discipline score, charts
- 🤖 **AI chart scoring** — Claude Sonnet 4.5 analyzes uploaded charts
- ⚙️ **User settings** — daily loss/profit limits with alerts
- 🧪 **Seed/wipe endpoints** — for easy local testing

## 🛠 Tech Stack

| Category | Technology |
|----------|-----------|
| Framework | ASP.NET Core (.NET 10) |
| Database | PostgreSQL + EF Core |
| Architecture | Clean Architecture (5 layers) |
| Pattern | CQRS via MediatR |
| Auth | Auth0 (JWT bearer) |
| AI | Anthropic Claude Sonnet 4.5 |
| Storage | Local filesystem (S3-ready interface) |

## 🏗 Architecture

```
TradingJournal.Api              ← Controllers, DI, middleware
TradingJournal.Application      ← MediatR handlers (commands & queries)
TradingJournal.Domain           ← Entities, repository interfaces
TradingJournal.Infrastructure   ← EF Core, repositories, external services
TradingJournal.Contract         ← DTOs, request/response models
```

Dependencies flow inward: Api → Application → Domain. Infrastructure implements Domain interfaces.

## 🚀 Getting Started

### Prerequisites

- .NET 10 SDK
- PostgreSQL 14+
- Auth0 account with an API configured
- Anthropic API key (for AI scoring)

### Setup

1. **Configure user secrets**
   ```bash
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=tradingjournal;Username=postgres;Password=YOUR_PASSWORD" --project TradingJournal.Api

   dotnet user-secrets set "Auth0:Domain" "your-tenant.auth0.com" --project TradingJournal.Api
   dotnet user-secrets set "Auth0:Audience" "https://trading-journal-api" --project TradingJournal.Api

   dotnet user-secrets set "Anthropic:ApiKey" "sk-ant-..." --project TradingJournal.Api
   ```

2. **Apply database migrations**
   ```bash
   dotnet ef database update --project TradingJournal.Api
   ```

3. **Run the API**
   ```bash
   dotnet run --project TradingJournal.Api --launch-profile https
   ```

4. API available at [https://localhost:7160](https://localhost:7160) — Swagger at `/swagger`

## 🔌 Key Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/v1/dashboard` | Dashboard stats + charts |
| GET | `/api/v1/trades` | List trades (with filters) |
| GET | `/api/v1/trades/{id}` | Trade detail |
| POST | `/api/v1/trades` | Log new trade (with files + AI scoring) |
| DELETE | `/api/v1/trades/{id}` | Delete a trade |
| GET | `/api/v1/users/me` | Current user info |
| PUT | `/api/v1/users/limits` | Update daily limits |

All endpoints require `Authorization: Bearer <jwt>` header.

## 🔗 Related

- [Frontend repository](https://github.com/jovityl/trading-journal-frontend)
