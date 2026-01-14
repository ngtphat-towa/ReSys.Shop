<#
.SYNOPSIS
    ReSys.Shop Local Development Runner
.DESCRIPTION
    Flexible script to start/stop/manage local development services.
    Supports multiple run modes: Target presets, individual flags, and interactive mode.
.EXAMPLE
    .\run-all-local.ps1                         # Start all services
    .\run-all-local.ps1 shop-full               # Start preset combination
    .\run-all-local.ps1 -Db -Api -Gateway       # Start specific services via flags
    .\run-all-local.ps1 -Interactive            # Interactive menu selection
    .\run-all-local.ps1 -Action Stop            # Stop all running services
    .\run-all-local.ps1 -Action Status          # Check service status
#>

[CmdletBinding(DefaultParameterSetName = 'Target')]
param(
    # --- Target Mode (Presets) ---
    [Parameter(ParameterSetName = 'Target', Position = 0)]
    [ValidateSet(
        "all", "db", "ml", "api", "gateway", "apps", 
        "backend", "frontend", "services", "infrastructure", 
        "dev-api", "ui-test", "shop-full", "admin-full", 
        "core-logic", "web-no-ml", "external-db", "identity"
    )]
    [string]$Target,

    # --- Flag Mode (Individual Services) ---
    [Parameter(ParameterSetName = 'Flags')]
    [switch]$Database,
    
    [Parameter(ParameterSetName = 'Flags')]
    [switch]$ML,
    
    [Parameter(ParameterSetName = 'Flags')]
    [switch]$Api,
    
    [Parameter(ParameterSetName = 'Flags')]
    [switch]$Gateway,
    
    [Parameter(ParameterSetName = 'Flags')]
    [switch]$Shop,
    
    [Parameter(ParameterSetName = 'Flags')]
    [switch]$Admin,

    [Parameter(ParameterSetName = 'Flags')]
    [switch]$Identity,

    # --- Interactive Mode ---
    [Parameter(ParameterSetName = 'Interactive')]
    [switch]$Interactive,

    # --- Action Mode ---
    [ValidateSet("Start", "Stop", "Restart", "Status")]
    [string]$Action = "Start",

    # --- Options ---
    [switch]$Detached,       # Run without new windows (background)
    [switch]$DryRun,         # Show what would be done without executing
    [switch]$Help            # Show help
)

$ErrorActionPreference = "Stop"
$script:ServicesStarted = @()

# --- Service Definitions ---
$script:ServiceDefinitions = @{
    "db" = @{
        Name        = "PostgreSQL (Docker)"
        Color       = "Cyan"
        WindowTitle = "PostgreSQL"
        Type        = "Docker"
    }
    "ml" = @{
        Name        = "ML Service (Python)"
        Color       = "Magenta"
        WindowTitle = "ML Service"
        Type        = "Python"
    }
    "api" = @{
        Name        = "Backend API (.NET)"
        Color       = "Green"
        WindowTitle = "Backend API"
        Type        = "DotNet"
    }
    "gateway" = @{
        Name        = "Gateway (.NET)"
        Color       = "Yellow"
        WindowTitle = "Gateway"
        Type        = "DotNet"
    }
    "shop" = @{
        Name        = "Shop App (Vue)"
        Color       = "Blue"
        WindowTitle = "Shop App"
        Type        = "Node"
    }
    "admin" = @{
        Name        = "Admin App (Vue)"
        Color       = "DarkCyan"
        WindowTitle = "Admin App"
        Type        = "Node"
    }
    "identity" = @{
        Name        = "Identity Service (.NET)"
        Color       = "Cyan"
        WindowTitle = "Identity Service"
        Type        = "DotNet"
    }
}

# --- Preset Combinations ---
$script:Presets = @{
    "all"            = @("db", "ml", "api", "gateway", "shop", "admin", "identity")
    "shop-full"      = @("db", "api", "gateway", "shop", "identity")
    "admin-full"     = @("db", "ml", "api", "gateway", "admin", "identity")
    "core-logic"     = @("db", "ml", "api", "gateway")
    "web-no-ml"      = @("db", "api", "gateway", "shop", "admin")
    "external-db"    = @("ml", "api", "gateway", "shop", "admin")
    "backend"        = @("db", "ml", "api")
    "frontend"       = @("shop", "admin")
    "services"       = @("ml", "api", "gateway")
    "infrastructure" = @("db", "ml")
    "dev-api"        = @("db", "api")
    "ui-test"        = @("gateway", "shop", "admin")
    "apps"           = @("shop", "admin")
}

# --- Helper Functions ---

function Write-Banner {
    $banner = @"

  ____       ____             ____  _                 
 |  _ \ ___ / ___| _   _ ___ / ___|| |__   ___  _ __  
 | |_) / _ \___ \| | | / __| \___ \| '_ \ / _ \| '_ \ 
 |  _ <  __/___) | |_| \__ \  ___) | | | | (_) | |_) |
 |_| \_\___|____/ \__, |___/ |____/|_| |_|\___/| .__/ 
                  |___/                        |_|    
  Local Development Runner v2.0

"@
    Write-Host $banner -ForegroundColor Cyan
}

function Write-ServiceStatus {
    param([string]$ServiceKey, [string]$Status, [string]$Details = "")
    
    $def = $script:ServiceDefinitions[$ServiceKey]
    $statusColor = switch ($Status) {
        "Starting"  { "Yellow" }
        "Running"   { "Green" }
        "Stopped"   { "Red" }
        "NotFound"  { "DarkGray" }
        default     { "White" }
    }
    
    $icon = switch ($Status) {
        "Starting"  { "[~]" }
        "Running"   { "[OK]" }
        "Stopped"   { "[X]" }
        "NotFound"  { "[?]" }
        default     { "[ ]" }
    }
    
    Write-Host "$icon " -ForegroundColor $statusColor -NoNewline
    Write-Host "$($def.Name)" -ForegroundColor $def.Color -NoNewline
    if ($Details) {
        Write-Host " - $Details" -ForegroundColor DarkGray
    } else {
        Write-Host ""
    }
}

function Show-Help {
    Write-Banner
    Write-Host "USAGE:" -ForegroundColor Yellow
    Write-Host "  .\run-all-local.ps1 [Target] [Options]" -ForegroundColor White
    Write-Host "  .\run-all-local.ps1 -Flags [Options]" -ForegroundColor White
    Write-Host "  .\run-all-local.ps1 -Interactive" -ForegroundColor White
    Write-Host ""
    
    Write-Host "PRESETS:" -ForegroundColor Yellow
    Write-Host "  all            - All services (default)" -ForegroundColor Gray
    Write-Host "  shop-full      - Customer experience: db, api, gateway, shop" -ForegroundColor Gray
    Write-Host "  admin-full     - Admin experience: db, ml, api, gateway, admin" -ForegroundColor Gray
    Write-Host "  core-logic     - Headless backend: db, ml, api, gateway" -ForegroundColor Gray
    Write-Host "  web-no-ml      - Standard web (no AI): db, api, gateway, shop, admin" -ForegroundColor Gray
    Write-Host "  external-db    - Cloud DB testing: ml, api, gateway, shop, admin" -ForegroundColor Gray
    Write-Host "  backend        - Backend only: db, ml, api" -ForegroundColor Gray
    Write-Host "  frontend       - Frontend only: shop, admin" -ForegroundColor Gray
    Write-Host "  services       - All services (no db): ml, api, gateway" -ForegroundColor Gray
    Write-Host "  dev-api        - API development: db, api" -ForegroundColor Gray
    Write-Host ""
    
    Write-Host "FLAGS (mix and match):" -ForegroundColor Yellow
    Write-Host "  -Database      - PostgreSQL database (Docker)" -ForegroundColor Gray
    Write-Host "  -ML            - Python ML service" -ForegroundColor Gray
    Write-Host "  -Api           - .NET Backend API" -ForegroundColor Gray
    Write-Host "  -Gateway       - .NET Gateway" -ForegroundColor Gray
    Write-Host "  -Shop          - Vue Shop application" -ForegroundColor Gray
    Write-Host "  -Admin         - Vue Admin application" -ForegroundColor Gray
    Write-Host ""
    
    Write-Host "ACTIONS:" -ForegroundColor Yellow
    Write-Host "  -Action Start   - Start services (default)" -ForegroundColor Gray
    Write-Host "  -Action Stop    - Stop all running services" -ForegroundColor Gray
    Write-Host "  -Action Restart - Restart services" -ForegroundColor Gray
    Write-Host "  -Action Status  - Show service status" -ForegroundColor Gray
    Write-Host ""
    
    Write-Host "OPTIONS:" -ForegroundColor Yellow
    Write-Host "  -Detached      - Run in background (no new windows)" -ForegroundColor Gray
    Write-Host "  -DryRun        - Show what would be done" -ForegroundColor Gray
    Write-Host "  -Interactive   - Interactive menu selection" -ForegroundColor Gray
    Write-Host "  -Help          - Show this help" -ForegroundColor Gray
    Write-Host ""
    
    Write-Host "EXAMPLES:" -ForegroundColor Yellow
    Write-Host "  .\run-all-local.ps1                      # Start all" -ForegroundColor DarkGray
    Write-Host "  .\run-all-local.ps1 shop-full            # Customer stack" -ForegroundColor DarkGray
    Write-Host "  .\run-all-local.ps1 -Db -Api -Shop       # Specific services" -ForegroundColor DarkGray
    Write-Host "  .\run-all-local.ps1 -Action Stop         # Stop all" -ForegroundColor DarkGray
    Write-Host "  .\run-all-local.ps1 -Interactive         # Menu mode" -ForegroundColor DarkGray
    Write-Host ""
}

function Show-InteractiveMenu {
    Write-Banner
    Write-Host "Select services to start:" -ForegroundColor Yellow
    Write-Host ""
    
    Write-Host "  PRESETS:" -ForegroundColor Cyan
    Write-Host "   1) all          - Everything" -ForegroundColor White
    Write-Host "   2) shop-full    - Customer experience stack" -ForegroundColor White
    Write-Host "   3) admin-full   - Admin experience stack" -ForegroundColor White
    Write-Host "   4) core-logic   - Headless backend" -ForegroundColor White
    Write-Host "   5) web-no-ml    - Standard web (no AI)" -ForegroundColor White
    Write-Host "   6) dev-api      - API development" -ForegroundColor White
    Write-Host ""
    
    Write-Host "  INDIVIDUAL:" -ForegroundColor Cyan
    Write-Host "   d) db       m) ml       a) api" -ForegroundColor White
    Write-Host "   g) gateway  s) shop     n) admin" -ForegroundColor White
    Write-Host ""
    
    Write-Host "  ACTIONS:" -ForegroundColor Cyan
    Write-Host "   x) Stop all services" -ForegroundColor White
    Write-Host "   t) Status check" -ForegroundColor White
    Write-Host "   q) Quit" -ForegroundColor White
    Write-Host ""
    
    $selection = Read-Host "Enter selection (e.g., '2' or 'dags' for db+api+gateway+shop)"
    
    if ($selection -eq 'q') {
        Write-Host "Goodbye!" -ForegroundColor Green
        return @()
    }
    
    if ($selection -eq 'x') {
        Stop-AllServices
        return @()
    }
    
    if ($selection -eq 't') {
        Get-ServiceStatus
        return @()
    }
    
    $services = @()
    
    # Handle preset numbers
    switch ($selection) {
        "1" { return $script:Presets["all"] }
        "2" { return $script:Presets["shop-full"] }
        "3" { return $script:Presets["admin-full"] }
        "4" { return $script:Presets["core-logic"] }
        "5" { return $script:Presets["web-no-ml"] }
        "6" { return $script:Presets["dev-api"] }
    }
    
    # Handle individual letters
    foreach ($char in $selection.ToCharArray()) {
        switch ($char) {
            'd' { $services += "db" }
            'm' { $services += "ml" }
            'a' { $services += "api" }
            'g' { $services += "gateway" }
            's' { $services += "shop" }
            'n' { $services += "admin" }
        }
    }
    
    return $services | Select-Object -Unique
}

# --- Service Start Functions ---

function Start-ServiceByKey {
    param(
        [string]$Key,
        [bool]$Detached = $false,
        [bool]$DryRun = $false
    )
    
    $def = $script:ServiceDefinitions[$Key]
    Write-Host "--- Starting $($def.Name) ---" -ForegroundColor $def.Color
    
    if ($DryRun) {
        Write-Host "  [DRY-RUN] Would start $($def.Name)" -ForegroundColor DarkGray
        return
    }
    
    switch ($Key) {
        "db" {
            docker-compose -f infrastructure/database/docker-compose.db.yml up -d
        }
        "ml" {
            if ($Detached) {
                Start-Process powershell -WindowStyle Hidden -ArgumentList "-Command", "cd src/services/ReSys.ML; .\.venv\Scripts\activate; python src/main.py"
            } else {
                Start-Process powershell -ArgumentList "-NoExit", "-Command", "`$Host.UI.RawUI.WindowTitle='$($def.WindowTitle)'; cd src/services/ReSys.ML; .\.venv\Scripts\activate; python src/main.py"
            }
        }
        "api" {
            $env:ConnectionStrings__shopdb = "Host=localhost;Database=shopdb;Username=postgres;Password=postgres"
            $env:MlSettings__ServiceUrl = "http://localhost:8000"
            if ($Detached) {
                Start-Process powershell -WindowStyle Hidden -ArgumentList "-Command", "dotnet run --project src/services/ReSys.Api/ReSys.Api.csproj"
            } else {
                Start-Process powershell -ArgumentList "-NoExit", "-Command", "`$Host.UI.RawUI.WindowTitle='$($def.WindowTitle)'; dotnet run --project src/services/ReSys.Api/ReSys.Api.csproj"
            }
        }
        "gateway" {
            $env:ServiceEndpoints__ApiUrl = "https://localhost:5001/"
            $env:ServiceEndpoints__MlUrl = "http://localhost:8000/"
            if ($Detached) {
                Start-Process powershell -WindowStyle Hidden -ArgumentList "-Command", "dotnet run --project src/services/ReSys.Gateway/ReSys.Gateway.csproj"
            } else {
                Start-Process powershell -ArgumentList "-NoExit", "-Command", "`$Host.UI.RawUI.WindowTitle='$($def.WindowTitle)'; dotnet run --project src/services/ReSys.Gateway/ReSys.Gateway.csproj"
            }
        }
        "shop" {
            if ($Detached) {
                Start-Process powershell -WindowStyle Hidden -ArgumentList "-Command", "cd src/apps/ReSys.Shop; npm run dev"
            } else {
                Start-Process powershell -ArgumentList "-NoExit", "-Command", "`$Host.UI.RawUI.WindowTitle='$($def.WindowTitle)'; cd src/apps/ReSys.Shop; npm run dev"
            }
        }
        "admin" {
            if ($Detached) {
                Start-Process powershell -WindowStyle Hidden -ArgumentList "-Command", "cd src/apps/ReSys.Admin; npm run dev"
            } else {
                Start-Process powershell -ArgumentList "-NoExit", "-Command", "`$Host.UI.RawUI.WindowTitle='$($def.WindowTitle)'; cd src/apps/ReSys.Admin; npm run dev"
            }
        }
        "identity" {
            $env:ConnectionStrings__shopdb = "Host=localhost;Database=shopdb;Username=postgres;Password=postgres"
            if ($Detached) {
                Start-Process powershell -WindowStyle Hidden -ArgumentList "-Command", "dotnet run --project src/services/ReSys.Identity/ReSys.Identity.csproj"
            } else {
                Start-Process powershell -ArgumentList "-NoExit", "-Command", "`$Host.UI.RawUI.WindowTitle='$($def.WindowTitle)'; dotnet run --project src/services/ReSys.Identity/ReSys.Identity.csproj"
            }
        }
    }
    
    $script:ServicesStarted += $Key
}

function Start-Services {
    param([string[]]$Services)
    
    Write-Banner
    Write-Host "Starting services: $($Services -join ', ')" -ForegroundColor Yellow
    Write-Host ""
    
    foreach ($svc in $Services) {
        Start-ServiceByKey -Key $svc -Detached $Detached -DryRun $DryRun
        Start-Sleep -Milliseconds 500
    }
    
    Write-Host ""
    Write-Host "Startup completed!" -ForegroundColor Green
    if (-not $Detached) {
        Write-Host "Check individual console windows for logs." -ForegroundColor Gray
    }
}

# --- Service Stop Functions ---

function Stop-AllServices {
    Write-Banner
    Write-Host "Stopping all services..." -ForegroundColor Yellow
    Write-Host ""
    
    # Stop Docker containers
    Write-Host "Stopping PostgreSQL (Docker)..." -ForegroundColor Cyan
    docker-compose -f infrastructure/database/docker-compose.db.yml down 2>$null
    
    # Kill processes by window title
    $windowTitles = @("ML Service", "Backend API", "Gateway", "Shop App", "Admin App")
    foreach ($title in $windowTitles) {
        $processes = Get-Process powershell -ErrorAction SilentlyContinue | Where-Object { $_.MainWindowTitle -eq $title }
        if ($processes) {
            Write-Host "Stopping $title..." -ForegroundColor Yellow
            $processes | Stop-Process -Force
        }
    }
    
    # Kill by port (fallback)
    $ports = @(8000, 5001, 5002, 5173, 5174)
    foreach ($port in $ports) {
        $connection = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue
        if ($connection) {
            $process = Get-Process -Id $connection.OwningProcess -ErrorAction SilentlyContinue
            if ($process) {
                Write-Host "Stopping process on port $port ($($process.ProcessName))..." -ForegroundColor Yellow
                Stop-Process -Id $connection.OwningProcess -Force -ErrorAction SilentlyContinue
            }
        }
    }
    
    Write-Host ""
    Write-Host "All services stopped." -ForegroundColor Green
}

# --- Service Status Functions ---

function Get-ServiceStatus {
    Write-Banner
    Write-Host "Service Status:" -ForegroundColor Yellow
    Write-Host ""
    
    # Check Docker
    $dockerRunning = docker ps --filter "name=postgres" --format "{{.Names}}" 2>$null
    if ($dockerRunning) {
        Write-ServiceStatus -ServiceKey "db" -Status "Running" -Details "Container: $dockerRunning"
    } else {
        Write-ServiceStatus -ServiceKey "db" -Status "Stopped"
    }
    
    # Check ports
    $portChecks = @{
        "ml"      = 8000
        "api"     = 5001
        "gateway" = 5002
        "shop"    = 5173
        "admin"   = 5174
        "identity" = 5074
    }
    
    foreach ($svc in $portChecks.Keys) {
        $port = $portChecks[$svc]
        $connection = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue | Where-Object State -eq "Listen"
        if ($connection) {
            $process = Get-Process -Id $connection.OwningProcess -ErrorAction SilentlyContinue
            Write-ServiceStatus -ServiceKey $svc -Status "Running" -Details "Port $port (PID: $($connection.OwningProcess))"
        } else {
            Write-ServiceStatus -ServiceKey $svc -Status "Stopped"
        }
    }
    
    Write-Host ""
}

# --- Main Execution ---

if ($Help) {
    Show-Help
    exit 0
}

# Determine which services to act on
$servicesToRun = @()

switch ($PSCmdlet.ParameterSetName) {
    'Interactive' {
        $servicesToRun = Show-InteractiveMenu
        if ($servicesToRun.Count -eq 0) {
            exit 0
        }
    }
    'Flags' {
        if ($Database) { $servicesToRun += "db" }
        if ($ML)       { $servicesToRun += "ml" }
        if ($Api)     { $servicesToRun += "api" }
        if ($Gateway) { $servicesToRun += "gateway" }
        if ($Shop)    { $servicesToRun += "shop" }
        if ($Admin)   { $servicesToRun += "admin" }
        
        if ($servicesToRun.Count -eq 0) {
            Write-Host "No services specified. Use -Help for usage." -ForegroundColor Red
            exit 1
        }
    }
    'Target' {
        if (-not $Target) {
            $Target = "all"
        }
        
        # Handle single service targets
        if ($script:Presets.ContainsKey($Target)) {
            $servicesToRun = $script:Presets[$Target]
        } elseif ($script:ServiceDefinitions.ContainsKey($Target)) {
            $servicesToRun = @($Target)
        }
    }
}

# Execute the action
switch ($Action) {
    "Start" {
        Start-Services -Services $servicesToRun
    }
    "Stop" {
        Stop-AllServices
    }
    "Restart" {
        Stop-AllServices
        Start-Sleep -Seconds 2
        Start-Services -Services $servicesToRun
    }
    "Status" {
        Get-ServiceStatus
    }
}
