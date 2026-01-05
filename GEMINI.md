# ReSys.Shop - Project Context

## Project Overview
**ReSys.Shop** is a modern, full-stack e-commerce platform featuring an AI-driven recommendation system. It is designed as a modular monorepo, utilizing .NET 9 for the backend, Vue 3 for the frontend, and Python for machine learning capabilities. The entire system is orchestrated for local development using **.NET Aspire**.

## Architecture & Tech Stack

### Backend (.NET 9)
*   **Structure:** Vertical Slice Architecture (using **Carter**) + Clean Architecture principles.
*   **Key Libraries:** MediatR (CQRS), FluentValidation, ErrorOr (Result Pattern), Entity Framework Core.
*   **Database:** PostgreSQL with `pgvector` extension for vector similarity search.
*   **Services:**
    *   `ReSys.Api`: Main business logic and data access.
    *   `ReSys.Gateway`: YARP-based gateway entry point.

### Frontend (Vue 3)
*   **Apps:**
    *   `ReSys.Shop`: Customer-facing storefront.
    *   `ReSys.Admin`: Administration panel.
*   **Tech:** Vite, TypeScript, Tailwind CSS 4, PrimeVue 4, Pinia, Axios.
*   **Conventions:** Feature-based folder structure, kebab-case naming.

### Machine Learning (Python)
*   **Service:** `ReSys.ML` (FastAPI).
*   **Tech:** PyTorch, Sentence Transformers.
*   **Function:** Generates vector embeddings for products to enable semantic search and recommendations.

### Infrastructure
*   **Orchestration:** .NET Aspire (`ReSys.AppHost`) manages all services and containers for local dev.
*   **Containerization:** Docker is used for PostgreSQL and Python services.

## Building and Running

### Prerequisites
*   .NET 9.0 SDK
*   Node.js (v20.19.0+ or >=22.12.0)
*   Docker Desktop
*   Python 3.10+

### Key Commands

**Start Everything (Recommended):**
The `.NET Aspire` host launches all services (DB, API, ML, Frontend) and a dashboard.
```bash
dotnet run --project infrastructure/aspire/ReSys.AppHost
```

**Using Helper Script (PowerShell):**
Provides an interactive menu or specific startup modes.
```powershell
.\scripts\run-all-local.ps1              # Start all
.\scripts\run-all-local.ps1 -Interactive # Interactive menu
.\scripts\run-all-local.ps1 shop-full    # Shop + API + DB
```

**Frontend Development:**
To run a frontend app in isolation (requires API running):
```bash
cd apps/ReSys.Shop
npm install
npm run dev
```

**Database Setup:**
Database is automatically managed by Aspire/Docker, but can be started manually:
```bash
docker-compose -f infrastructure/database/docker-compose.db.yml up -d
```

## Development Conventions

*   **Vertical Slices:** Code is organized by feature (e.g., `Features/Products`, `Features/Users`) rather than technical layer. Each feature folder contains its own Models, Handlers, and Validators.
*   **Error Handling:** Exceptions are avoided for control flow. The backend uses the **Result Pattern** (`ErrorOr<T>`) to return explicit success/failure states.
*   **API Design:** RESTful APIs using `snake_case` JSON responses.
*   **Frontend Structure:**
    *   Views are located in `src/views` but grouped by feature context.
    *   Shared logic resides in `src/shared` or specific `features` directories.
    *   Forms use `VeeValidate` with `Zod` schemas.

## Key Configuration Files
*   `infrastructure/aspire/ReSys.AppHost/Program.cs`: Service orchestration logic.
*   `services/ReSys.Api/Program.cs`: Backend DI and middleware setup.
*   `apps/ReSys.Shop/vite.config.ts`: Frontend build configuration.
*   `scripts/run-all-local.ps1`: Main development helper script.
