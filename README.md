# CloudBoard

A modern project management application inspired by Azure DevOps Boards, built with .NET 8 and Vue 3.

## 🛠️ Tech Stack

### Backend
- **Framework**: ASP.NET Core 8.0
- **Database**: PostgreSQL 16
- **ORM**: Entity Framework Core 8.0
- **Authentication**: ASP.NET Core Identity + JWT Bearer
- **API Documentation**: Swagger/OpenAPI

### Frontend
- **Framework**: Vue 3 (Composition API)
- **Language**: TypeScript 5.9
- **Build Tool**: Vite 7.1
- **State Management**: Pinia 3.0
- **Routing**: Vue Router 4.6
- **HTTP Client**: Axios 1.13
- **Styling**: Tailwind CSS 3.4
- **Testing**: Vitest + Playwright

### Infrastructure
- **Containerization**: Docker & Docker Compose
- **CI/CD**: GitHub Actions
- **Hosting**: Render.com (Backend & Frontend)
- **Database Hosting**: Render PostgreSQL

---

## 🏃 Getting Started

### Prerequisites
- Docker & Docker Compose (recommended)
- OR: .NET 8 SDK + Node.js 18+ + PostgreSQL 16

### Quick Start with Docker

1. **Clone the repository**
   ```bash
   git clone https://github.com/mhahnsjoe/CloudBoard.git
   cd CloudBoard
2. **Set up environment variables**

Create api/CloudBoard.Api/appsettings.Development.json:

{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=cloudboard;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Secret": "your-local-development-secret-min-32-characters",
    "Issuer": "CloudBoardAPI",
    "Audience": "CloudBoardClient"
  }
}
3. **Start the application**
docker-compose up --build

4.**Access the application**

Frontend: http://localhost:5173
Backend API: http://localhost:5000
Swagger UI: http://localhost:5000/swagger

5. **Create an account**

Navigate to http://localhost:5173/register
Register with email/password
Start creating projects!


## 📁 Project Structure
CloudBoard/
├── api/                          # Backend (.NET)
│   └── CloudBoard.Api/
│       ├── Controllers/          # REST API endpoints
│       ├── Models/               # Domain entities & DTOs
│       ├── Services/             # Business logic
│       ├── Data/                 # EF Core DbContext
│       └── Program.cs            # Application startup
├── frontend/                     # Frontend (Vue 3)
│   └── src/
│       ├── components/           # Vue components
│       ├── stores/               # Pinia state management
│       ├── services/             # API client
│       ├── types/                # TypeScript interfaces
│       └── router/               # Vue Router config
├── docker-compose.yml            # Local development setup
└── README.md                     # This file

## 🔒 Security
JWT tokens with 7-day expiration
Passwords hashed with ASP.NET Core Identity (PBKDF2)
Environment-based configuration (no secrets in code)
CORS configured for specific origins
Protected API endpoints with [Authorize] attribute
User-isolated data (users only see their own projects)
