# ReSys.Shop - Quick Start Guide

## 🚀 One-Command Startup

```powershell
# Start everything
.\scripts\run-all-local.ps1

# Interactive menu
.\scripts\run-all-local.ps1 -Interactive
```

---

## 📦 Common Development Stacks

| Command | What it starts | Best for |
|---------|---------------|----------|
| `.\scripts\run-all-local.ps1 shop-full` | db, api, gateway, shop | Shop development |
| `.\scripts\run-all-local.ps1 admin-full` | db, ml, api, gateway, admin | Admin + ML development |
| `.\scripts\run-all-local.ps1 dev-api` | db, api | Backend API work |
| `.\scripts\run-all-local.ps1 frontend` | shop, admin | Frontend only |

---

## 🎛️ Service Management

```powershell
# Check what's running
.\scripts\run-all-local.ps1 -Action Status

# Stop all services
.\scripts\run-all-local.ps1 -Action Stop

# Restart
.\scripts\run-all-local.ps1 shop-full -Action Restart
```

---

## 🔧 Custom Combinations

```powershell
# Mix and match with flags
.\scripts\run-all-local.ps1 -Database -Api -Shop

# Available flags:
# -Database, -ML, -Api, -Gateway, -Shop, -Admin
```

---

## 📖 Full Documentation

- **Detailed Guide**: `docs/RUNNING_GUIDE.md`
- **Script Help**: `.\scripts\run-all-local.ps1 -Help`

---

## 🌐 Default Ports

| Service | Port | URL |
|---------|------|-----|
| PostgreSQL | 5432 | localhost:5432 |
| ML Service | 8000 | http://localhost:8000 |
| Backend API | 5001 | https://localhost:5001 |
| Gateway | 5002 | https://localhost:5002 |
| Shop App | 5173 | http://localhost:5173 |
| Admin App | 5174 | http://localhost:5174 |

---

## 🆘 Troubleshooting

**Services not starting?**
```powershell
# Clean restart
.\scripts\run-all-local.ps1 -Action Stop
Start-Sleep -Seconds 2
.\scripts\run-all-local.ps1 shop-full
```

**Port conflicts?**
```powershell
# Check what's using ports
.\scripts\run-all-local.ps1 -Action Status
```

**Preview changes?**
```powershell
# Dry run shows what would start
.\scripts\run-all-local.ps1 shop-full -DryRun
```
