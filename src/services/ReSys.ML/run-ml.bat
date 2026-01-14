@echo off
setlocal
cd %~dp0

REM Check for uv
where uv >nul 2>nul
if %errorlevel% neq 0 (
    echo uv is not installed. Installing via pip...
    pip install uv
    if %errorlevel% neq 0 (
        echo Failed to install uv. Please install it manually.
        exit /b 1
    )
)

if not exist .venv (
    echo Creating virtual environment with uv...
    uv venv .venv
)

call .venv\Scripts\activate

if "%1"=="notebook" (
    echo Starting Jupyter Lab...
    uv pip install jupyterlab
    jupyter lab
) else (
    echo Installing dependencies with uv...
    uv pip install -r requirements.txt
    
    echo Starting ML Service...
    cd src
    uvicorn main:app --host 0.0.0.0 --port 8000 --reload
)