# ReSys.Shop - Quick Start Guide

## 🚀 One-Command Startup (Standalone)

```powershell
# Start everything in background
.\scripts\local\Run-Local.ps1 all -Detached

# Check status
.\scripts\local\Run-Local.ps1 -Action Status
```

---

## 📦 Common Development Stacks

| Command | Starts | Best for |
|---------|--------|----------|
| `.\scripts\local\Run-Local.ps1 shop-full` | db, api, gateway, shop | Consumer Features |
| `.\scripts\local\Run-Local.ps1 admin-full` | db, ml, api, gateway, admin | Admin & AI Features |
| `.\scripts\local\Run-Local.ps1 dev-api` | db, api | Backend logic only |
| `.\scripts\local\Run-Local.ps1 frontend` | shop, admin | UI work only |

---

## 🎛️ Service Management

```powershell
# View health and PIDs
.\scripts\local\Run-Local.ps1 -Action Status

# Stop all processes
.\scripts\local\Run-Local.ps1 -Action Stop

# Forceful port cleanup (Emergency)
.\scripts\local\Clear-Ports.ps1
```

---

## 🌐 Default Ports (Local Context)

| Service | Port | URL |
|---------|------|-----|
| **Gateway** | 7073 | https://localhost:7073 |
| **Backend API** | 5001 | https://localhost:5001 |
| **ML Service** | 8000 | http://localhost:8000 |
| **Shop App** | 5174 | http://localhost:5174 |
| **Admin App** | 5173 | http://localhost:5173 |
| **PostgreSQL** | 5432 | localhost:5432 |

---

## 📖 Related Documentation

- **Full Manual**: `scripts/local/README.md`
- **Thesis Suite**: `scripts/thesis/README.md`
- **Architecture**: `README.md`

---

## 🆘 Troubleshooting

**Address already in use?**
Use the "Scorched Earth" utility to kill all zombie listeners:
```powershell
.\scripts\local\Clear-Ports.ps1
```

**Services crashing immediately?**
Run without `-Detached` to see the live console logs:
```powershell
.\scripts\local\Run-Local.ps1 all
```

**First time setup?**
Ensure your machine has the required SDKs and dependencies:
```powershell
.\scripts\local\Initialize-Env.ps1
```