# ReSys.Shop: Local Development Suite (Manual & Orchestration)

This directory contains the core toolchain for running the ReSys.Shop ecosystem in **Standalone (Local) Mode**. This mode is essential for developers who need to debug services individually, reduce system resource usage, or work without the abstraction of .NET Aspire.

---

## 🛠️ 1. Architecture & "Manual Wiring"

In Standalone mode, we bypass Aspire's dynamic proxy system. Instead, we use **static port assignment** and **explicit environment injection**.

### How Service Discovery is Handled
When `Run-Local.ps1` starts a service, it injects environment variables that override the settings in `appsettings.json`.
- **YARP Gateway**: The script tells the Gateway exactly where each service is listening by overriding YARP Clusters.
  - *Example*: `${env:ReverseProxy__Clusters__api-cluster__Destinations__destination1__Address} = "https://localhost:5001"`
- **API to ML**: The API is told where the Python ML service is via `MlSettings__ServiceUrl`.
- **Database**: All .NET services are pointed to the Docker-hosted PostgreSQL via `ConnectionStrings__shopdb`.

---

## 📜 2. Detailed Script Catalog

### 🔄 `Initialize-Env.ps1`
**Purpose**: Prepares the host machine for the ReSys.Shop environment.
- **Actions**:
  1. **.NET Check**: Validates that .NET 10 SDK is installed (required for modern features).
  2. **Python Setup**: Uses `uv` to create a virtual environment in `src/services/ReSys.ML`. `uv` is preferred over standard `venv` for its 10x-100x speed improvement in dependency resolution.
  3. **Package Restore**: Performs root-level `dotnet restore` and individual `npm install` for frontend apps.
- **When to run**: First-time setup, after pulling major changes, or when dependencies change.

### 🚀 `Run-Local.ps1`
**Purpose**: The central orchestrator for starting and managing services.
- **Argument Reference**:
  - `[Target]`: Positional argument. Can be a **Preset** (`all`, `shop-full`, `admin-full`, `core-logic`, `backend`, `frontend`) or a **Service Name** (`db`, `ml`, `api`, `gateway`, `shop`, `admin`).
  - `-Action`: 
    - `Start`: Launches services.
    - `Stop`: Stops all project processes.
    - `Restart`: Sequential Stop -> Start.
    - `Status`: Probes ports to verify health.
  - `-Detached`: Launches services in hidden windows. Logs are redirected internally (see `Debug-ServiceStartup.ps1`).
  - `-Interactive`: Launches a menu-driven selection mode.
- **Internal Logic**: The script explicitly uses `--launch-profile https` for .NET services to ensure they bind to the correct ports (5001, 7073).

### 🧹 `Clear-Ports.ps1`
**Purpose**: Forceful cleanup of zombie processes.
- **Why it's needed**: Sometimes a crash leaves a `dotnet.exe` or `node.exe` process running in the background, even if the terminal window is closed. This prevents new instances from binding to the port (Error 10048).
- **Action**: It identifies the Process ID (PID) holding the project's ports and kills them immediately.

### 🔍 `Get-PortStatus.ps1` & `Test-ApiPort.ps1`
**Purpose**: Diagnostic tools for connectivity issues.
- `Get-PortStatus.ps1`: Maps every active port to its service name and PID.
- `Test-ApiPort.ps1`: An advanced check that uses `netstat` and `Get-NetTCPConnection` to detect hidden IPv6 listeners.

---

## 🚦 3. Common Workflows

### Standard Developer Loop
1. **Reset Environment**: `.\scripts\local\Clear-Ports.ps1`
2. **Start Services**: `.\scripts\local\Run-Local.ps1 all -Detached`
3. **Verify Health**: `.\scripts\local\Run-Local.ps1 -Action Status` (Expect 6/6 services [OK]).

### Focused Feature Work
If working only on the Shop frontend:
1. `.\scripts\local\Run-Local.ps1 db,api,gateway -Detached`
2. `.\scripts\local\Run-Local.ps1 shop` (Runs in foreground for Hot Module Replacement logs).

---

## 🕳️ 4. Pitfall Gallery (Real-World Solutions)

During the stabilization of this context, several complex issues were identified and resolved:

### 1. The PowerShell Hyphen Syntax
**The Problem**: Commands like `$env:api-cluster = "..."` would crash the script.
**The Cause**: PowerShell interprets `-` as a subtraction operator.
**The Fix**: Use `${env:ReverseProxy__Clusters__api-cluster...}` syntax.

### 2. IPv6 Address-Agnostic Probing
**The Problem**: Status check shows `[X]` but the service is definitely running.
**The Cause**: Modern .NET binds to `[::1]` (IPv6). If the status check only looks for `127.0.0.1` (IPv4), it reports a false negative.
**The Fix**: Probing logic was updated to search by port number across all address families.

### 3. Terminal-Input Hang
**The Problem**: API service starts but hangs or stops immediately.
**The Cause**: Wrapping `dotnet run` inside a `powershell -Command` can cause the process to wait for terminal input.
**The Fix**: Services now use `Start-Process dotnet` directly to isolate the execution environment.

### 4. Database Password Mismatch
**The Problem**: API fails to connect to DB with "Authentication failed".
**The Cause**: Discrepancy between Docker container variables (`POSTGRES_PASSWORD`) and script overrides.
**The Fix**: Standardized on `password` across all `Run-Local.ps1` configurations.

---

## 📋 5. Port Reference Registry

| Service | Port (HTTP) | Port (HTTPS) | Protocol |
|---------|-------------|--------------|----------|
| **PostgreSQL** | 5432 | - | TCP |
| **Backend API** | 5000 | **5001** | HTTPS (Primary) |
| **Gateway** | 5129 | **7073** | HTTPS (Primary) |
| **ML Service** | 8000 | - | HTTP (FastAPI) |
| **Shop App** | 5174 | - | Vite/Node |
| **Admin App** | 5173 | - | Vite/Node |
