# CloudBoard

A project management application inspired by Azure DevOps Boards, built to plan and track software projects with hierarchical work items, sprint planning, and burndown analytics.

<!-- TODO: Add 2-3 screenshots here once the app is deployed
![Dashboard](docs/screenshots/dashboard.png)
![Kanban Board](docs/screenshots/kanban.png)
![Sprint Burndown](docs/screenshots/burndown.png)
-->

## Features

- **Hierarchical Work Items** — Epic → Feature → PBI → Task with parent-child validation and cycle detection
- **Kanban Boards** — drag-and-drop cards between status columns with real-time state updates
- **Sprint Planning** — create sprints, drag backlog items into iterations, track capacity
- **Burndown Charts** — daily remaining-hours tracking with ideal vs. actual trend lines (Chart.js)
- **Backlog Management** — prioritized backlog with drag reordering, type/status filtering, and inline editing
- **Team Collaboration** — create teams, invite members, share projects across a team
- **Authentication** — JWT-based auth with ASP.NET Core Identity (register, login, protected routes)
- **API Documentation** — Swagger/OpenAPI available at `/swagger` on the running API

## Tech Stack

### Backend

| Layer | Technology |
|-------|-----------|
| Framework | ASP.NET Core (.NET 10) |
| Database | PostgreSQL 16 |
| ORM | Entity Framework Core 9 |
| Auth | ASP.NET Core Identity + JWT Bearer |
| Logging | Serilog (structured, file + console sinks) |
| Testing | xUnit, FluentAssertions, Moq, Testcontainers |
| API Docs | Swagger / OpenAPI |

### Frontend

| Layer | Technology |
|-------|-----------|
| Framework | Vue 3 (Composition API + `<script setup>`) |
| Language | TypeScript 5.9 |
| Build Tool | Vite 7 |
| State | Pinia 3 |
| Routing | Vue Router 4 |
| HTTP | Axios |
| Styling | Tailwind CSS 3.4 |
| Charts | Chart.js + vue-chartjs |
| Drag & Drop | vue-draggable-plus |
| Testing | Vitest + Vue Test Utils |

### Infrastructure

| Concern | Technology |
|---------|-----------|
| Containerization | Docker & Docker Compose |
| CI/CD | GitHub Actions (build, test, deploy) |
| Hosting | Render.com (Backend + Frontend) |
| Database Hosting | Render PostgreSQL |

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│  Frontend — Vue 3 + Pinia + Tailwind                    │
│  SPA served via Render Static Site                      │
└────────────────────────┬────────────────────────────────┘
                         │ REST / JSON
┌────────────────────────▼────────────────────────────────┐
│  API — ASP.NET Core 10                                  │
│  ┌────────────┐  ┌──────────────┐  ┌────────────────┐   │
│  │Controllers │→ │   Services   │→ │ Repositories   │   │
│  └────────────┘  │ (validation, │  │ (data access,  │   │
│                  │  business    │  │  EF Core)      │   │
│                  │  logic)      │  │                │   │
│                  └──────────────┘  └───────┬────────┘   │
│  Auth: JWT + Identity                      │            │
│  Logging: Serilog (structured)             │            │
│  Errors: Result<T> + global middleware     │            │
└────────────────────────────────────────────┼────────────┘
                                             │
┌────────────────────────────────────────────▼────────────┐
│  PostgreSQL 16                                          │
│  Tables: Users, Projects, Boards, WorkItems, Sprints,   │
│          Teams, TeamMembers, WorkItemHistory            │
└─────────────────────────────────────────────────────────┘
```

Key patterns documented in [`docs/architecture/decisions/`](docs/architecture/decisions/):

- **ADR-001** — Data access strategy (direct DbContext → later migrated to Repository pattern)
- **ADR-002** — Error handling (Result&lt;T&gt; pattern + global exception middleware)
- **ADR-003** — Logging strategy (Serilog, structured, no correlation IDs in monolith)
- **ADR-004** — Testing strategy (Testcontainers over mocked DbContext)
- **ADR-005** — Technical debt register (centralized, not inline TODOs)
- **ADR-006** — Repository pattern implementation (with rationale for skipping Unit of Work)

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) (22+ recommended)
- [Docker](https://www.docker.com/) (for PostgreSQL and integration tests)
- A PostgreSQL 16 instance (or use Docker Compose below)

### Option 1 — Docker Compose (Recommended)

Starts PostgreSQL and the API together:

```bash
git clone https://github.com/mhahnsjoe/CloudBoard.git
cd CloudBoard

# Start PostgreSQL
docker compose up db -d

# Apply database migrations
cd api/CloudBoard.Api
dotnet tool restore
dotnet ef database update
cd ../..

# Start the API (runs on http://localhost:5154)
cd api/CloudBoard.Api
dotnet run
```

In a second terminal, start the frontend:

```bash
cd frontend
npm install
npm run dev          # runs on http://localhost:5173
```

### Option 2 — Full Docker Build

Builds the entire app into a single container:

```bash
docker compose up --build
```

The app will be available at `http://localhost:8080`.

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ConnectionStrings__Default` | PostgreSQL connection string | see `docker-compose.yml` |
| `Jwt__Secret` | JWT signing key | set in `appsettings.Development.json` |
| `Jwt__Issuer` | JWT issuer | `CloudBoard` |
| `Jwt__Audience` | JWT audience | `CloudBoard` |

## Running Tests

### Backend

```bash
# Unit tests
dotnet test api/CloudBoard.Api.Tests --filter "FullyQualifiedName!~Integration"

# Integration tests (requires Docker for Testcontainers)
dotnet test api/CloudBoard.Api.Tests --filter "FullyQualifiedName~Integration"

# All tests
dotnet test api/CloudBoard.Api.Tests
```

### Frontend

```bash
cd frontend
npm run test:unit         # watch mode
npm run test:unit -- --run  # single run (CI)
npm run type-check        # TypeScript checking
npm run lint              # ESLint
```

## CI/CD

GitHub Actions runs on every push to `main` and `feature/*` branches:

1. **Backend** — restore, build, unit tests, integration tests (Testcontainers)
2. **Frontend** — install, unit tests, production build
3. **Deploy** — EF Core migrations + deploy to Render (main branch only)

## Project Structure

```
CloudBoard/
├── api/
│   ├── CloudBoard.Api/
│   │   ├── Controllers/       # API endpoints
│   │   ├── Services/          # Business logic
│   │   ├── Repositories/      # Data access (EF Core)
│   │   ├── Models/            # Entities & DTOs
│   │   ├── Data/              # DbContext & migrations
│   │   ├── Common/            # Result<T>, extensions
│   │   └── Middleware/        # Error handling, logging
│   └── CloudBoard.Api.Tests/
│       ├── Services/          # Unit tests (Moq, FluentAssertions)
│       ├── Repositories/      # Repository unit tests
│       └── Integration/       # Full API tests (Testcontainers)
├── frontend/
│   └── src/
│       ├── components/        # Vue components (views, modals, UI)
│       ├── composables/       # Reusable logic (useToast, useModal, etc.)
│       ├── services/          # API client (Axios)
│       ├── stores/            # Pinia state management
│       ├── types/             # TypeScript interfaces
│       └── views/             # Route-level pages
├── docs/
│   └── architecture/
│       └── decisions/         # ADRs (001–006)
├── docker-compose.yml
├── Dockerfile
└── .github/workflows/build.yml
```

## Roadmap

Planned improvements (not in current scope):

- **OAuth Authentication** — GitHub / Microsoft provider for professional login flow
- **Real-time Updates** — SignalR for live board updates across clients
- **Audit Trail** — full history of who changed what and when
- **Global Search** — search across projects, boards, and work items
- **E2E Tests** — Playwright browser tests for critical user flows

## License

This project is for portfolio and personal use.
