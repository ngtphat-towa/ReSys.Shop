$env:ConnectionStrings__shopdb = "Host=localhost;Database=shopdb;Username=postgres;Password=password"
$env:MlSettings__ServiceUrl = "http://localhost:8000"

Write-Host "Starting API..."
Start-Process dotnet -ArgumentList "run --project src/services/ReSys.Api/ReSys.Api.csproj" -RedirectStandardOutput "api.log" -RedirectStandardError "api_err.log" -NoNewWindow

Write-Host "Starting ML..."
$mlDir = Resolve-Path "src/services/ReSys.ML"
Start-Process powershell -ArgumentList "-Command", "cd $mlDir; .\.venv\Scripts\activate; uvicorn src.main:app --host 0.0.0.0 --port 8000" -RedirectStandardOutput "ml.log" -RedirectStandardError "ml_err.log" -NoNewWindow

Start-Sleep -Seconds 20

Write-Host "--- API Log ---"
if (Test-Path api.log) { Get-Content api.log -Tail 20 }
Write-Host "--- API Err ---"
if (Test-Path api_err.log) { Get-Content api_err.log -Tail 20 }

Write-Host "--- ML Log ---"
if (Test-Path ml.log) { Get-Content ml.log -Tail 20 }
Write-Host "--- ML Err ---"
if (Test-Path ml_err.log) { Get-Content ml_err.log -Tail 20 }