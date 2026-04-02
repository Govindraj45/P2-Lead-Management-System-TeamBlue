# Lead Management System — TeamBlue

A production-grade **Lead Lifecycle & Sales Pipeline** system built with Clean Architecture, CQRS, and deployed to Azure.

## Live Deployment

| Service  | URL |
|----------|-----|
| Backend API | `https://lms-api-app-c2a944a6.azurewebsites.net` |
| Frontend    | `https://lms-frontend-app-f4025f7a.azurewebsites.net` |

## Tech Stack

| Layer        | Technology                              |
|--------------|-----------------------------------------|
| Backend      | ASP.NET Core Web API (.NET 8)           |
| Frontend     | React 19 (Vite + Nginx)                |
| Database     | Azure SQL Server (EF Core Code First)   |
| Caching      | Azure Cache for Redis                   |
| Auth         | JWT + Role-Based Authorization (BCrypt) |
| Containers   | Docker + Docker Compose                 |
| Registry     | Azure Container Registry (ACR)          |
| Hosting      | Azure App Service (Linux containers)    |
| CI/CD        | Azure DevOps Pipelines                  |
| Orchestration| Kubernetes manifests (Kustomize)        |
| Pattern      | CQRS (without MediatR)                  |

## Database Schema (ER Diagram)

```
┌──────────────────────┐       ┌──────────────────────────┐       ┌─────────────────────────┐
│        Users         │       │          Leads           │       │     Interactions        │
├──────────────────────┤       ├──────────────────────────┤       ├─────────────────────────┤
│ PK UserId       int  │──┐    │ PK LeadId          int   │──┐    │ PK InteractionId   int  │
│    Email     nv(256) │  │    │    Name          nv(200) │  │    │    InteractionType  str │
│    PasswordHash  str │  │    │    Email         nv(256) │  │    │    Notes            str │
│    FullName      str │  │    │    Phone             str │  │    │    InteractionDate   dt │
│    Role       nv(50) │  │    │    Company           str │  │    │    FollowUpDate      dt │
│    CreatedDate    dt │  │    │    Position          str │  └───>│ FK LeadId           int │
└──────────────────────┘  │    │    Status        nv(50)  │       └─────────────────────────┘
   Roles: SalesRep,       │    │    Source       nv(100)  │        OnDelete: Cascade
   SalesManager, Admin    │    │    Priority      nv(50)  │
                          │    │    CreatedDate       dt  │
                          │    │    ModifiedDate      dt  │
                          │    │    ConvertedDate     dt  │
                          └───>│ FK AssignedSalesRepId int│
                               └──────────────────────────┘
                                OnDelete: SetNull
```

**Relationships:**
- **Users 1:M Leads** — A SalesRep user can be assigned many leads (`SetNull` on delete)
- **Leads 1:M Interactions** — A lead can have many interactions (`Cascade` on delete)

## Features

- Lead CRUD with strict input validation
- Lead status lifecycle: `New → Contacted → Qualified → Converted` (no skipping stages)
- Lead conversion with business rules (only Qualified → Converted, Manager/Admin only)
- Interaction tracking with date validation (no future dates, follow-up > interaction date)
- Analytics & reporting with Redis caching (5-min TTL, auto-invalidation on mutations)
- Role-based access control: `SalesRep`, `SalesManager`, `Admin`
- Pagination, filtering, and search
- EF Core indexes on Email, Status, Source, AssignedSalesRepId

## API Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/register` | — | Register a new user |
| POST | `/api/auth/login` | — | Login, returns JWT |
| GET | `/api/leads` | All Roles | List leads (paginated) |
| GET | `/api/leads/{id}` | All Roles | Get lead by ID |
| POST | `/api/leads` | All Roles | Create a lead |
| PUT | `/api/leads/{id}` | All Roles | Update a lead |
| PATCH | `/api/leads/{id}/status` | All Roles | Update lead status |
| DELETE | `/api/leads/{id}` | All Roles | Delete a lead |
| POST | `/api/leads/{id}/convert` | Manager/Admin | Convert lead to customer |
| GET | `/api/interactions/lead/{leadId}` | All Roles | Get interactions for a lead |
| POST | `/api/interactions` | All Roles | Create an interaction |
| GET | `/api/leads/analytics/by-source` | All Roles | Leads grouped by source |
| GET | `/api/leads/analytics/by-status` | All Roles | Leads grouped by status |
| GET | `/api/leads/analytics/conversion-rate` | All Roles | Conversion rate |
| GET | `/api/leads/analytics/by-salesrep` | All Roles | Leads per sales rep |
| GET | `/api/reports/status-distribution` | All Roles | Status distribution report |
| GET | `/api/reports/by-source` | All Roles | Source report |
| GET | `/api/reports/conversion-rate` | All Roles | Conversion rate report |
| GET | `/api/reports/by-salesrep` | All Roles | Sales rep report |

## Azure Infrastructure

Provisioned via `azure/provision.sh`:

| Resource | Service |
|----------|---------|
| Resource Group | `rg-lead-management` (centralus) |
| Container Registry | `lmscontainerregistry.azurecr.io` |
| SQL Server + DB | `CRM_LeadManagement` on Azure SQL |
| Redis Cache | Azure Cache for Redis (Basic C0) |
| App Service Plan | Linux B1 |
| Backend App | Linux container from ACR |
| Frontend App | Linux container from ACR (Nginx) |

## Getting Started

### Prerequisites
- .NET 8 SDK
- Node.js 18+
- Docker & Docker Compose
- Azure CLI (for cloud deployment)

### Run with Docker Compose (Local)
```bash
docker-compose up
```
This starts: SQL Server, Redis, Backend API (port 5000), Frontend (port 3000).

### Run Locally (without Docker)

**Backend:**
```bash
cd LeadManagementBackend
dotnet restore
dotnet run --project LeadManagementApp
```

**Frontend:**
```bash
cd LeadManagementFrontend
npm install
npm run dev
```

### Deploy to Azure
```bash
# 1. Login and provision resources
az login
chmod +x azure/provision.sh
./azure/provision.sh

# 2. Build & push Docker images
az acr login --name lmscontainerregistry
docker build --platform linux/amd64 -t lmscontainerregistry.azurecr.io/lms-api:latest ./LeadManagementBackend
docker build --platform linux/amd64 -t lmscontainerregistry.azurecr.io/lms-frontend:latest ./LeadManagementFrontend
docker push lmscontainerregistry.azurecr.io/lms-api:latest
docker push lmscontainerregistry.azurecr.io/lms-frontend:latest

# 3. Restart App Services to pull new images
az webapp restart --name <backend-app-name> --resource-group rg-lead-management
az webapp restart --name <frontend-app-name> --resource-group rg-lead-management
```

### Default Test Credentials
| Role | Email | Password |
|------|-------|----------|
| Admin | `admin@leadcrm.com` | `Admin@123` |
| Sales Manager | `manager@leadcrm.com` | `Manager@123` |
| Sales Rep | `rep@leadcrm.com` | `Rep@123` |

## Architecture

- **Clean Architecture** — strict separation of concerns
- **CQRS** — Commands (write) and Queries (read) fully separated, no MediatR, manual DI
- **Repository Pattern** — `ILeadRepository`, `IInteractionRepository` with EF Core implementations
- **JWT Authentication** — role-based policies (`SalesRep`, `SalesManager`, `Admin`)
- **Redis Caching** — analytics cached for 5 min, auto-invalidated on lead/interaction mutations
- **EF Core Code First** — migrations auto-applied on startup

## Project Structure

```
├── azure/                       # Azure provisioning script
├── azure-pipelines.yml          # CI/CD pipeline definition
├── docker-compose.yml           # Local dev orchestration
├── k8s/                         # Kubernetes manifests (Kustomize)
│   ├── base/                    # Base configs (api, frontend, redis, sql, ingress)
│   └── overlays/                # Environment-specific overrides (dev, prod)
├── LeadManagementBackend/
│   ├── LeadManagementApp/       # Main API (Minimal API + CQRS handlers)
│   │   ├── Auth/                # TokenService, SeedDataService
│   │   ├── Data/                # DbContext, EF repositories
│   │   ├── Features/            # CQRS commands & queries (Leads, Interactions, Reports)
│   │   ├── Interfaces/          # Repository abstractions
│   │   ├── Logic/               # ReportService, LeadService
│   │   ├── Migrations/          # EF Core migrations
│   │   └── Models/              # User, Lead, Interaction entities
│   ├── LeadManagementShared/    # Shared models & interfaces for microservices
│   ├── LeadManagementGateway/   # API Gateway (Consul-based)
│   ├── LeadManagement*Service/  # Microservice variants (Leads, Interactions, Reports, SalesReps)
│   └── LeadManagementTests/     # xUnit tests
└── LeadManagementFrontend/
    └── src/                     # React SPA (Vite + Nginx)
```

## Branching Strategy

- `feature/<name>` — New features
- `fix/<issue>` — Bug fixes
- `refactor/<scope>` — Code improvements
- No direct commits to `main` — PRs required

## Team

**TeamBlue**
