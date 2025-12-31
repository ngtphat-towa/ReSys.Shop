@echo off
setlocal
cd %~dp0

if not exist venv (
    echo Creating virtual environment...
    python -m venv venv
)

call venv\Scripts\activate

if "%1"=="notebook" (
    echo Starting Jupyter Lab...
    jupyter lab
) else (
    echo Installing dependencies...
    pip install -r requirements.txt
    echo Starting ML Service...
    cd src
    uvicorn main:app --host 0.0.0.0 --port 8000 --reload
)