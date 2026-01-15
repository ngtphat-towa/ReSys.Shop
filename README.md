# ReSys.Shop

ReSys.Shop is a modern, high-performance e-commerce ecosystem built with **Vertical Slice Architecture**. It leverages a .NET 10 backend, a high-speed Vue 3 frontend, and a Python-based Machine Learning service for advanced product discovery.

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512bd4.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![Vue 3](https://img.shields.io/badge/Vue-3.5-4fc08d.svg)](https://vuejs.org/)
[![Python 3.12](https://img.shields.io/badge/Python-3.12-3776ab.svg)](https://www.python.org/)
[![Tailwind 4](https://img.shields.io/badge/Tailwind-4.0-38bdf8.svg)](https://tailwindcss.com/)
[![Vite](https://img.shields.io/badge/Vite-646CFF?logo=vite&logoColor=white)](https://vitejs.dev/)
[![Docker](https://img.shields.io/badge/Docker-2496ED?logo=docker&logoColor=white)](https://www.docker.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-4169E1?logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![TypeScript](https://img.shields.io/badge/TypeScript-3178C6?logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
[![PrimeVue](https://img.shields.io/badge/PrimeVue-4.0-4fc08d.svg)](https://primevue.org/)
[![OpenTelemetry](https://img.shields.io/badge/OpenTelemetry-000000?logo=opentelemetry&logoColor=white)](https://opentelemetry.io/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

---

## 📖 Table of Contents
- [Architecture & Design](#-architecture--design)
- [System Components](#-system-components)
- [Project Folders](#-project-folders)
- [Prerequisites](#-prerequisites)
- [How to Run](#-how-to-run)
- [Testing & Quality Assurance](#-testing--quality-assurance)
- [Guide: Adding New Features](#-guide-adding-new-features)
- [Local Development Suite](#-local-development-suite)
- [Troubleshooting](#-troubleshooting)
- [License](#-license)

---

## 🏗️ Architecture & Design

### Vertical Slice Architecture (VSA)
Unlike traditional N-Tier architecture that groups code by technical role (Controllers, Services, Repositories), this project groups code by **Feature**. Each folder in the core logic represents a standalone business capability (e.g., "Product Catalog"). This maximizes cohesion and minimizes the cognitive load when modifying a specific feature.

### Backend Strategy
- **MediatR**: Implements the Mediator pattern to decouple the "What" (Request) from the "How" (Handler).
- **Carter**: A thin wrapper over ASP.NET Core Minimal APIs that allows for elegant, module-based route registration.
- **FluentValidation**: Centralized, robust validation logic for all incoming feature requests.
- **Result Pattern**: Uses `ErrorOr<T>` to handle business failures as values instead of expensive exceptions.

### Frontend Strategy
- **Module-Based**: Frontend components and stores are grouped by business context to match the backend slices.
- **Tailwind CSS 4**: Utilizes the latest utility-first CSS framework for ultra-fast UI development.
- **PrimeVue 4**: Professional-grade UI component library using the "Aura" design system.

---

## 🏗️ System Components

### 1. The Frontend (UI)
*   **ReSys.Shop**: The consumer storefront. Optimized for performance and SEO.
*   **ReSys.Admin**: A feature-rich management portal for staff to manage inventory, users, and ML models.

### 2. The Backend (Logic)
*   **ReSys.Api**: The engine of the project. Manages persistence, business rules, and external integrations.
*   **ReSys.Identity**: A dedicated security service handling JWT issuance, Role-Based Access Control (RBAC), and user management.
*   **ReSys.Gateway**: Powered by **Microsoft YARP**, it serves as the single point of entry, providing routing, SSL termination, and centralized health monitoring.

### 3. The AI Service (Intelligence)
*   **ReSys.ML**: Python/FastAPI service for vector embeddings and image analysis.

---

## 📂 Project Folders

```text
.
├── src/                        # Primary Source Code
│   ├── apps/                   # Frontend Projects (Vite + Vue)
│   │   ├── ReSys.Admin/        # Staff Management panel
│   │   └── ReSys.Shop/         # Customer Storefront
│   ├── services/               # Backend Microservices
│   │   ├── ReSys.Api/          # Core Business API (.NET 10)
│   │   ├── ReSys.Gateway/      # YARP Router & Entry Point
│   │   ├── ReSys.Identity/     # Auth & Identity Authority
│   │   └── ReSys.ML/           # Python AI Service
│   ├── libs/                   # Shared Class Libraries
│   │   ├── ReSys.Core/         # Business Logic (Common, Domain, Features)
│   │   ├── ReSys.Infrastructure/ # Clients (Imaging, Ml, Notifications, Persistence)
│   │   ├── ReSys.Migrations/   # EF Core Migration History
│   │   └── ReSys.Shared/       # Telemetry, Shared Models & Constants
│   └── aspire/                 # Orchestration Layer
│       ├── ReSys.AppHost/      # .NET Aspire Entry Point
│       └── ReSys.ServiceDefaults/ # Shared Configurations
├── infrastructure/             # Local Infrastructure (Docker)
│   ├── database/               # PostgreSQL & pgvector config
│   ├── mail/                   # Papercut SMTP Config
│   └── storage/                # Local storage simulation
├── scripts/                    # Utility Suite
│   ├── local/                  # Standalone Tools (Run-Local, Clear-Ports)
│   └── thesis/                 # CTU Thesis Generators
├── tests/                      # Automated Test Suite
│   ├── aspire/                 # Orchestration Tests
│   │   └── ReSys.AppHost.Tests # Web & connectivity tests
│   ├── libs/                   # Library Unit Tests
│   │   ├── ReSys.Core.UnitTests # Logic & Behavior tests
│   │   └── ReSys.Infrastructure.UnitTests # Client & DB tests
│   ├── services/               # Microservice Tests
│   │   ├── ReSys.Api.IntegrationTests # End-to-end API tests
│   │   ├── ReSys.Api.UnitTests        # Controller & Middleware tests
│   │   ├── ReSys.Identity.IntegrationTests
│   │   └── ReSys.Identity.UnitTests
│   └── TestAssets/             # Mock files (images, data) for tests
├── docs/                       # Diagrams, guidelines & views
└── ReSys.Shop.sln              # Solution Entry Point
```

---

## 🚀 Prerequisites

To run this project, you need the following installed:
- **.NET 10 SDK**: [Download](https://dotnet.microsoft.com/download/dotnet/10.0)
- **Node.js v20+**: [Download](https://nodejs.org/)
- **Python 3.12+**: [Download](https://www.python.org/)
- **Docker Desktop**: [Download](https://www.docker.com/)
- **Typst CLI** (Optional): Only if you are using the thesis generators.

---

## 🏁 How to Run

### Option A: Using .NET Aspire (Best for Full Development)
This is the easiest way to run the entire stack. It automatically handles database startup, service discovery, and provides a unified dashboard.
```bash
dotnet run --project src/aspire/ReSys.AppHost
```

### Option B: Standalone Mode (Best for Focused Debugging)
Use the **Local Context** tools to run services as independent processes. This is lighter on system resources.
```powershell
# 1. Initialize environment (One-time)
.\scripts\local\Initialize-Env.ps1

# 2. Run local infrastructure
docker-compose -f infrastructure/database/docker-compose.db.yml up -d

# 3. Launch everything in background
.\scripts\local\Run-Local.ps1 all -Detached
```

---

## 🧪 Testing & Quality Assurance

Quality is a first-class citizen in ReSys.Shop, using a multi-layer strategy:

- **Unit Tests**: Found in `tests/libs` and `tests/services`. Focuses on business logic in `ReSys.Core`.
- **Integration Tests**: Comprehensive API tests in `tests/services` verifying HTTP responses and DB state.
- **Aspire Tests**: Located in `tests/aspire`, verifying orchestration and service discovery.

**Execution:**
```bash
# Run all tests
dotnet test

# Run tests for a specific project
dotnet test tests/services/ReSys.Api.UnitTests
```

---

## ✨ Guide: Adding New Features

Follow the project's **Vertical Slice** pattern:

### 1. Backend Implementation
- Create a folder in `src/libs/ReSys.Core/Features/[FeatureName]`.
- Define your **Request**, **Handler**, and **Validator** in this folder.
- Register your API routes in `src/services/ReSys.Api/Features/[FeatureName]Module.cs`.

### 2. Frontend Implementation
- Group Vue components and Pinia stores in a corresponding feature folder under `src/apps/[AppName]/src/modules`.
- Adhere to the `kebab-case` naming convention for all files.

---

## 🛠️ Local Development Suite

The `scripts/local/` directory contains a professional-grade toolchain for managing standalone services:
- **Run-Local.ps1**: Orchestrates service startup with 15+ environment variable overrides for manual service discovery.
- **Clear-Ports.ps1**: Forcefully terminates zombie processes on development ports (5000-8000).
- **Get-PortStatus.ps1**: Diagnostic tool mapping active listeners to service names.

See [scripts/local/README.md](scripts/local/README.md) for full technical details.

---

## 🛠 Troubleshooting

| Issue | Solution |
|-------|----------|
| **Port 10048 (In Use)** | Run `.\scripts\local\Clear-Ports.ps1`. |
| **API Health 502/404** | Ensure the Gateway is running and `PathRemovePrefix` is correctly configured in `appsettings.json`. |
| **Database Auth Failure** | Standard password is `password`. Verify the `POSTGRES_PASSWORD` env var in the Docker config. |

---

## 📄 License
ReSys.Shop is open-source software licensed under the [MIT License](LICENSE).
