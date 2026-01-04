# ReSys.Shop

A modern, high-performance e-commerce ecosystem built with a modular .NET backend, a sophisticated Vue 3 frontend, and integrated AI capabilities.

[![.NET 9](https://img.shields.io/badge/.NET-9.0-512bd4.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791.svg)](https://www.postgresql.org/)
[![Python](https://img.shields.io/badge/Python-3.10-3776ab.svg)](https://www.python.org/)
[![Vue 3](https://img.shields.io/badge/Vue-3.5-4fc08d.svg)](https://vuejs.org/)
[![PrimeVue 4](https://img.shields.io/badge/PrimeVue-4.0-4fc08d.svg)](https://primevue.org/)
[![Tailwind 4](https://img.shields.io/badge/Tailwind-4.0-38bdf8.svg)](https://tailwindcss.com/)

## 🏗️ Architecture & Technical Stack

ReSys is engineered for scalability, maintainability, and high performance using **Clean Architecture** and **Vertical Slice** patterns.

### 🔷 Backend: The Powerhouse (.NET 9)
- **Vertical Slices**: Organized via [Carter](https://github.com/CarterCommunity/Carter), keeping related logic, routes, and validation together.
- **CQRS**: Driven by [MediatR](https://github.com/jbogard/MediatR) to decouple read and write side logic.
- **Validation**: [FluentValidation](https://fluentvalidation.net/) integrated via MediatR Pipeline Behaviors, ensuring data integrity before reaching handlers.
- **Error Handling**: Implements the **Result Pattern** with [ErrorOr<T>](https://github.com/amantinband/error-or), providing type-safe success/error paths and standardizing on **RFC 7807 Problem Details**.
- **Persistence**: **Entity Framework Core** with **Npgsql**, utilizing advanced features like `snake_case` naming conventions and `pgvector` for AI search.
- **Serialization**: Customized JSON serialization (System.Text.Json & Newtonsoft) for consistent `snake_case` responses across all microservices.

### 🔷 Frontend: The Interface (Vue 3)
- **Composition API**: Clean, stateful logic using modern Vue patterns.
- **Design System**: [PrimeVue 4](https://primevue.org/) utilizing the **Aura** preset for a modern, accessible UI.
- **Styling**: [Tailwind CSS 4](https://tailwindcss.com/) with semantic mappings to the design system.
- **Infrastructure**:
    - **Optimized API Client**: Custom Axios implementation with interceptors for automatic response unwrapping and global "Smart Toast" notifications.
    - **Form Management**: High-performance validation bridge using [VeeValidate](https://vee-validate.logaretm.com/v4/) and [Zod](https://zod.dev/).
    - **Hierarchical Routing**: Dot-notation naming convention (`testing.examples.list`) enabling decoupled and predictable navigation.
    - **Dynamic Breadcrumbs**: Automated navigation trail generation based on route metadata and localized keys.
    - **Modular State**: Feature-encapsulated [Pinia](https://pinia.vuejs.org/) stores for efficient reactive data management.

### 🔷 AI & Machine Learning integration
- **FastAPI Sidecar**: A Python microservice providing high-performance REST endpoints for compute-intensive AI tasks.
- **Vector Search**: Automatic generation of **Vector Embeddings** for products and images, stored in **PostgreSQL (pgvector)**.
- **Semantic Similarity**: Real-time "You May Also Love" recommendations powered by cosine distance vector matching.

---

## 📂 Project Structure Guide

```text
├── apps/                   
│   ├── ReSys.Admin/        # Administrative dashboard with theme configurator and CRUD tools.
│   └── ReSys.Shop/         # Consumer storefront with high-performance catalog browsing.
├── services/               
│   ├── ReSys.Api/          # Central business API (Vertical Slices, Storage, and Logic).
│   ├── ReSys.Gateway/      # YARP-based Reverse Proxy providing a unified API entry point.
│   └── ReSys.ML/           # Python service for AI embeddings and image processing.
├── libs/                   
│   ├── ReSys.Core/         # Domain entities, MediatR logic, and core business abstractions.
│   └── ReSys.Infrastructure/ # Persistence (EF Core), File Storage, and Image implementations.
├── infrastructure/         
│   ├── aspire/             # .NET Aspire orchestration for local development and observability.
│   └── database/           # Docker Compose configurations for PostgreSQL and vector extensions.
└── tests/                  # xUnit integration and unit test suites for all layers.
```

---

## 🛠️ Developer Onboarding: The Golden Standard

When adding new features, follow the established patterns in the `testing/examples` module to ensure consistency:

1.  **Backend Slice**: 
    - Create a new directory in `ReSys.Core/Features/[FeatureName]`.
    - Define `Command`, `Query`, `Validator`, and `Handler` in a single file or folder.
    - Register the module in `ReSys.Api/Features` via `ICarterModule`.
2.  **Frontend Module**:
    - Directory Structure: Use `kebab-case` for all folder and file names.
    - **Types**: Define in `shared/api/[feature].types.ts`.
    - **Store**: Implement feature logic in `[feature].store.ts` using the Result Pattern.
    - **Locales**: Centralize all UI text in `[feature].locales.ts` following the `FeatureLocales` interface.
    - **Views**: Use kebab-case filenames (e.g., `example-list.vue`).

---

## 🚀 Getting Started

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js](https://nodejs.org/) (v20+ recommended)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Python 3.10+](https://www.python.org/)

### Quick Start

1.  **Start Local Infrastructure** (PostgreSQL + pgvector):
    ```bash
    docker-compose -f infrastructure/database/docker-compose.db.yml up -d
    ```

2.  **Run via .NET Aspire (Recommended)**:
    Launch the `ReSys.AppHost` project. This will start the Gateway, API, ML service, and both Vue apps.
    ```bash
    dotnet run --project infrastructure/aspire/ReSys.AppHost
    ```

3.  **Run Standalone (PowerShell)**:
    For more granular control over specific services:
    ```bash
    .\scripts\run-all-local.ps1 all
    ```

---

## 📖 Essential Documentation
- [Architecture Patterns](./docs/ARCHITECTURE_PATTERNS.md)
- [Deployment & Standalone Guide](./docs/RUNNING_GUIDE.md)
- [Reference UI Views](./docs/ARCHITECTURE_REFERENCE_VIEWS.md)
- [Thesis Guidelines](./docs/CTU_THESIS_GUIDELINES.md)
