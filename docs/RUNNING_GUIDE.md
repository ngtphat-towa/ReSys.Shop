# ReSys.Shop: Deployment & Running Guide

This document explains how to run the ReSys.Shop ecosystem individually (Standalone) without using .NET Aspire orchestration.

---

## 1. Prerequisites
*   **.NET 9 SDK**
*   **Node.js** (v20+)
*   **Python 3.10+**
*   **Docker Desktop** (for the database)

---

## 2. Infrastructure (The Database)
The system requires PostgreSQL with the `pgvector` extension.

### Run with Docker Compose
```powershell
docker-compose -f infrastructure/database/docker-compose.db.yml up -d
```
*   **Port:** 5432
*   **User:** postgres
*   **Password:** postgres
*   **Database:** shopdb

---

## 3. Backend Services (.NET)

### Configuration (Environment Variables)
When running standalone, you must override Aspire's service discovery. Set these variables or update `appsettings.Development.json`.

**For ReSys.Api:**
*   `ConnectionStrings__shopdb`: `Host=localhost;Database=shopdb;Username=postgres;Password=postgres`
*   `MlSettings__ServiceUrl`: `http://localhost:8000`

**For ReSys.Gateway:**
*   `ServiceEndpoints__ApiUrl`: `https://localhost:5001/`
*   `ServiceEndpoints__MlUrl`: `http://localhost:8000/`
*   `ReverseProxy__Clusters__api-cluster__Destinations__destination1__Address`: `https://localhost:5001`

### Running Commands
```powershell
# Run API
dotnet run --project services/ReSys.Api/ReSys.Api.csproj

# Run Gateway
dotnet run --project services/ReSys.Gateway/ReSys.Gateway.csproj
```

---

## 4. ML Service (Python)

### Setup
1. Create and activate virtual environment:
   ```powershell
   cd services/ReSys.ML
   python -m venv .venv
   .\.venv\Scripts\activate
   ```
2. Install dependencies:
   ```powershell
   pip install -r requirements.txt
   ```

### Running
```powershell
python src/main.py
```
*   **Default Port:** 8000

---

## 5. Frontend Apps (Vue.js)

### Setup
Run `npm install` in both `apps/ReSys.Shop` and `apps/ReSys.Admin`.

### Running
```powershell
# Shop
cd apps/ReSys.Shop
npm run dev

# Admin
cd apps/ReSys.Admin
npm run dev
```
*   **Shop Port:** 5173 (usually)
*   **Admin Port:** 5174 (usually)

---

## 6. Automated Local Script (Recommended)

We provide a flexible PowerShell script to manage your local development environment: `.\scripts\run-all-local.ps1`

### 6.1 Quick Start
```powershell
# Start all services (default)
.\scripts\run-all-local.ps1

# Start a specific preset
.\scripts\run-all-local.ps1 shop-full

# Interactive menu selection
.\scripts\run-all-local.ps1 -Interactive

# Check service status
.\scripts\run-all-local.ps1 -Action Status

# Stop all services
.\scripts\run-all-local.ps1 -Action Stop
```

### 6.2 Execution Modes

#### 🎯 Target Mode (Presets)
Use predefined service combinations:

```powershell
.\scripts\run-all-local.ps1 [preset-name]
```

**Available Presets:**
| Preset | Services | Use Case |
|--------|----------|----------|
| `all` | db, ml, api, gateway, shop, admin | Everything (default) |
| `shop-full` | db, api, gateway, shop | Customer experience stack |
| `admin-full` | db, ml, api, gateway, admin | Admin experience stack |
| `core-logic` | db, ml, api, gateway | Headless backend (no UI) |
| `web-no-ml` | db, api, gateway, shop, admin | Standard web (no AI) |
| `external-db` | ml, api, gateway, shop, admin | Cloud DB testing |
| `backend` | db, ml, api | Backend only |
| `frontend` | shop, admin | Frontend only |
| `services` | ml, api, gateway | All services (no db) |
| `dev-api` | db, api | API development |
| `ui-test` | gateway, shop, admin | UI testing |

**Examples:**
```powershell
# Work on shop features
.\scripts\run-all-local.ps1 shop-full

# Work on admin panel with ML
.\scripts\run-all-local.ps1 admin-full

# Test APIs without UI
.\scripts\run-all-local.ps1 core-logic
```

#### 🚩 Flag Mode (Mix & Match)
Select individual services:

```powershell
.\scripts\run-all-local.ps1 -Database -Api -Gateway -Shop
```

**Available Flags:**
- `-Database` - PostgreSQL database (Docker)
- `-ML` - Python ML service
- `-Api` - .NET Backend API
- `-Gateway` - .NET Gateway
- `-Shop` - Vue Shop application
- `-Admin` - Vue Admin application

**Examples:**
```powershell
# Just database and API
.\scripts\run-all-local.ps1 -Database -Api

# Frontend development (assumes backend is running elsewhere)
.\scripts\run-all-local.ps1 -Gateway -Shop -Admin

# Test ML integration
.\scripts\run-all-local.ps1 -Database -ML -Api
```

#### 💬 Interactive Mode
Menu-driven service selection:

```powershell
.\scripts\run-all-local.ps1 -Interactive
```

The interactive menu allows you to:
- Select presets by number (`1-6`)
- Select individual services by letter:
  - `d` = database
  - `m` = ml
  - `a` = api
  - `g` = gateway
  - `s` = shop
  - `n` = admin
- Combine letters: `dags` = db + api + gateway + shop
- `x` = Stop all services
- `t` = Status check
- `q` = Quit

### 6.3 Actions

Control service lifecycle:

```powershell
.\scripts\run-all-local.ps1 [target] -Action [Start|Stop|Restart|Status]
```

**Available Actions:**
- `Start` - Start services (default)
- `Stop` - Stop all running services
- `Restart` - Restart services
- `Status` - Show service status

**Examples:**
```powershell
# Check what's running
.\scripts\run-all-local.ps1 -Action Status

# Stop everything
.\scripts\run-all-local.ps1 -Action Stop

# Restart specific stack
.\scripts\run-all-local.ps1 shop-full -Action Restart
```

### 6.4 Additional Options

**Dry Run:**
Preview what would be started without executing:
```powershell
.\scripts\run-all-local.ps1 shop-full -DryRun
```

**Detached Mode:**
Run services in background (no new windows):
```powershell
.\scripts\run-all-local.ps1 shop-full -Detached
```

**Help:**
Show detailed usage information:
```powershell
.\scripts\run-all-local.ps1 -Help
```

### 6.5 Service Status Indicators

When checking status, you'll see:
- `[✓]` - Service is running
- `[✗]` - Service is stopped
- Port numbers and process IDs for running services

**Example Output:**
```
[✓] PostgreSQL (Docker) - Container: shopdb_postgres
[✓] ML Service (Python) - Port 8000 (PID: 12345)
[✓] Backend API (.NET) - Port 5001 (PID: 12346)
[✓] Gateway (.NET) - Port 5002 (PID: 12347)
[✓] Shop App (Vue) - Port 5173 (PID: 12348)
[✗] Admin App (Vue)
```

### 6.6 Common Workflows

**Full Stack Development:**
```powershell
.\scripts\run-all-local.ps1 all
```

**Shop Feature Development:**
```powershell
.\scripts\run-all-local.ps1 shop-full
```

**Admin Panel Development:**
```powershell
.\scripts\run-all-local.ps1 admin-full
```

**Backend API Development:**
```powershell
.\scripts\run-all-local.ps1 dev-api
```

**Frontend Only (backend running elsewhere/deployed):**
```powershell
.\scripts\run-all-local.ps1 frontend
```

**Clean Restart:**
```powershell
.\scripts\run-all-local.ps1 -Action Stop
Start-Sleep -Seconds 2
.\scripts\run-all-local.ps1 shop-full
```

---

## 7. Full Stack Docker (Advanced)
To run everything inside Docker without installing runtimes:
1. Ensure `Dockerfile` exists in each service.
2. Create a root `docker-compose.yml` (Optional, currently DB only).

```