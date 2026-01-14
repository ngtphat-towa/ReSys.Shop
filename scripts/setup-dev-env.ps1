<#
.SYNOPSIS
    ReSys.Shop Development Environment Setup
.DESCRIPTION
    Sets up the local development environment:
    - Checks for .NET 10 SDK
    - Installs/Checks 'uv' for Python
    - Sets up Python Virtual Environment and installs dependencies
    - Restores .NET packages
    - Installs Node.js dependencies
.EXAMPLE
    .\setup-dev-env.ps1
#>

$ErrorActionPreference = "Stop"

function Write-Step {
    param([string]$Message)
    Write-Host ">>> $Message" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "    [OK] $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "    [WARN] $Message" -ForegroundColor Yellow
}

Write-Host "Setting up ReSys.Shop Development Environment..." -ForegroundColor Magenta
Write-Host ""

# 1. Check .NET SDK
Write-Step "Checking .NET SDK..."
$dotnetVersion = dotnet --version
if ($dotnetVersion.StartsWith("10.")) {
    Write-Success "Found .NET SDK $dotnetVersion"
} else {
    Write-Warning "Found .NET SDK $dotnetVersion. This project targets .NET 10. Please ensure you have .NET 10 installed."
}

# 2. Setup Python (uv)
Write-Step "Setting up Python (ML Service)..."
if (-not (Get-Command "uv" -ErrorAction SilentlyContinue)) {
    Write-Warning "'uv' not found. Installing via pip..."
    try {
        pip install uv
        Write-Success "Installed uv"
    } catch {
        Write-Error "Failed to install uv. Please install it manually: https://github.com/astral-sh/uv"
    }
} else {
    Write-Success "Found uv"
}

$mlDir = Join-Path $PSScriptRoot "..\src\services\ReSys.ML"
Push-Location $mlDir
try {
    if (-not (Test-Path ".venv")) {
        Write-Host "    Creating virtual environment..." -ForegroundColor Gray
        uv venv .venv
    }
    
    Write-Host "    Installing Python dependencies..." -ForegroundColor Gray
    # uv pip install requires active venv or --python argument. 
    # We use --python to point to the venv explicitly.
    if ($IsWindows) {
        uv pip install -r requirements.txt --python .venv\Scripts\python.exe
    } else {
        uv pip install -r requirements.txt --python .venv/bin/python
    }
    Write-Success "Python dependencies installed"
} finally {
    Pop-Location
}

# 3. Restore .NET
Write-Step "Restoring .NET packages..."
dotnet restore
Write-Success ".NET packages restored"

# 4. Setup Node.js (Shop & Admin)
Write-Step "Setting up Node.js apps..."

$apps = @("ReSys.Shop", "ReSys.Admin")
foreach ($app in $apps) {
    $appDir = Join-Path $PSScriptRoot "..\src\apps\$app"
    if (Test-Path $appDir) {
        Push-Location $appDir
        Write-Host "    Installing dependencies for $app..." -ForegroundColor Gray
        try {
            npm install
            Write-Success "$app dependencies installed"
        } catch {
            Write-Warning "Failed to install dependencies for $app"
        } finally {
            Pop-Location
        }
    }
}

Write-Host ""
Write-Host "Setup Completed Successfully!" -ForegroundColor Green
Write-Host "You can now run the application using: .\scripts\run-all-local.ps1" -ForegroundColor Gray
