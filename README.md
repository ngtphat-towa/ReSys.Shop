# ReSys.Shop

An e-commerce project built with a .NET backend, a Vue 3 frontend, and a Python service for AI features.

[![.NET 9](https://img.shields.io/badge/.NET-9.0-512bd4.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)
[![.NET Aspire](https://img.shields.io/badge/.NET_Aspire-512BD4?logo=dotnet&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/aspire/)
[![Docker](https://img.shields.io/badge/Docker-2496ED?logo=docker&logoColor=white)](https://www.docker.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-4169E1?logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Python](https://img.shields.io/badge/Python-3.10-3776ab.svg)](https://www.python.org/)
[![FastAPI](https://img.shields.io/badge/FastAPI-009688?logo=fastapi&logoColor=white)](https://fastapi.tiangolo.com/)
[![Vue 3](https://img.shields.io/badge/Vue-3.5-4fc08d.svg)](https://vuejs.org/)
[![TypeScript](https://img.shields.io/badge/TypeScript-3178C6?logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
[![Vite](https://img.shields.io/badge/Vite-646CFF?logo=vite&logoColor=white)](https://vitejs.dev/)
[![PrimeVue 4](https://img.shields.io/badge/PrimeVue-4.0-4fc08d.svg)](https://primevue.org/)
[![Tailwind 4](https://img.shields.io/badge/Tailwind-4.0-38bdf8.svg)](https://tailwindcss.com/)
[![Pinia](https://img.shields.io/badge/Pinia-FFE148?logo=vue.js&logoColor=black)](https://pinia.vuejs.org/)

## How the Code is Organized

This project is organized by **Feature (Vertical Slices)** to keep it easy to maintain as it grows. 

Instead of putting all database files in one place and all UI files in another, we group everything belonging to one feature (like "Products" or "Examples") together. If you need to change how a specific feature works, you only need to look in one folder. This avoids searching through many different parts of the project to find related code.

### Backend (.NET 9)
- **Features**: Grouped into slices using [Carter](https://github.com/CarterCommunity/Carter).
- **Logic**: Uses [MediatR](https://github.com/jbogard/MediatR) to separate data reading from data writing.
- **Validation**: Checks input data using [FluentValidation](https://fluentvalidation.net/).
- **Errors**: Uses the **Result Pattern** (`ErrorOr<T>`) for predictable error handling instead of throwing exceptions.
- **Database**: **PostgreSQL** with EF Core. Uses `pgvector` for AI-based product searching.
- **Format**: All API responses use `snake_case` JSON.

### Frontend (Vue 3)
- **UI Components**: Built with [PrimeVue 4](https://primevue.org/) and the **Aura** theme.
- **Styling**: Uses [Tailwind CSS 4](https://tailwindcss.com/).
- **API Calls**: Custom Axios setup that handles data unwrapping and error notifications.
- **Forms**: Handles validation using [VeeValidate](https://vee-validate.logaretm.com/v4/) and [Zod](https://zod.dev/).
- **Routing**: Uses a nested naming system (e.g., `testing.examples.list`) to keep navigation organized.
- **Breadcrumbs**: Generates the navigation trail automatically from the current route.

### AI Features
- **ML Service**: A Python microservice (FastAPI) that creates vector embeddings for images and text.
- **Search**: Uses vector matching in the database to show "Similar Products" based on what the user is viewing.

---


## Project Folders

```text
├── apps/                   
│   ├── ReSys.Admin/        # Admin panel for managing products and settings.
│   └── ReSys.Shop/         # Storefront for customers to browse products.
├── services/               
│   ├── ReSys.Api/          # The main API handling business logic and storage.
│   ├── ReSys.Gateway/      # Single entry point for all API requests.
│   └── ReSys.ML/           # Python service for AI and image tasks.
├── libs/                   
│   ├── ReSys.Core/         # Shared business logic and database models.
│   └── ReSys.Infrastructure/ # Database and file storage implementations.
├── infrastructure/         
│   └── aspire/             # Local development setup using .NET Aspire.
└── tests/                  # Automated tests for the whole project.
```

---


## Guide for Adding New Features

When adding a new feature, follow the pattern used in the `testing/examples` folder:

1.  **Backend**:
    - Create a new folder in `ReSys.Core/Features`.
    - Add the logic (Handler) and the data check (Validator) in that folder.
    - Register the new routes in `ReSys.Api/Features`.
2.  **Frontend**:
    - Use `kebab-case` for all files and folders.
    - Define data types in `shared/api`.
    - Add UI text to a `locales.ts` file in the feature folder.
    - Create the pages (Views) using the shared components like breadcrumbs.

---


## How to Run

1. **Start the Database**:
   ```bash
   docker-compose -f infrastructure/database/docker-compose.db.yml up -d
   ```

2. **Run all services**:
   The easiest way is to run the `ReSys.AppHost` project using .NET Aspire.
   ```bash
   dotnet run --project infrastructure/aspire/ReSys.AppHost
   ```

3. **Alternative**:
   You can also use the PowerShell script to run specific parts of the project:
   ```bash
   .\scripts\run-all-local.ps1 all
   ```