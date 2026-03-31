# Lead Management System — TeamBlue

A production-style **Lead Lifecycle & Sales Pipeline** system built with clean architecture and CQRS.

## Tech Stack

| Layer       | Technology                        |
|-------------|-----------------------------------|
| Backend     | ASP.NET Core Web API (.NET 8)     |
| Frontend    | React (Vite)                      |
| Database    | SQL Server (EF Core Code First)   |
| Caching     | Redis                             |
| Auth        | JWT + Role-Based Authorization    |
| Containers  | Docker + Docker Compose           |
| Pattern     | CQRS (without MediatR)            |

## Features

- Lead CRUD with strict validation (FluentValidation)
- Lead status lifecycle management (New → Contacted → Qualified → Converted)
- Lead conversion with business rules enforcement
- Interaction tracking with date validation
- Sales rep management
- Analytics & reporting with Redis caching
- Role-based access (SalesRep, SalesManager, Admin)
- Pagination, filtering, and search

## Getting Started

### Prerequisites
- .NET 8 SDK
- Node.js 18+
- Docker & Docker Compose
- SQL Server
- Redis

### Run with Docker
```bash
docker-compose up
```

### Run Locally

**Backend:**
```bash
cd crm-backend
dotnet restore
dotnet run --project LeadManagementApp
```

**Frontend:**
```bash
cd crm-frontend
npm install
npm run dev
```

## Architecture

- **Clean Architecture** with strict separation of concerns
- **CQRS** — Commands (write) and Queries (read) are fully separated
- **Repository pattern** for data access
- **JWT authentication** with role-based policies

## Branching Strategy

- `feature/<name>` — New features
- `fix/<issue>` — Bug fixes
- `refactor/<scope>` — Code improvements
- No direct commits to `main` — PRs required

## Team

**TeamBlue**
