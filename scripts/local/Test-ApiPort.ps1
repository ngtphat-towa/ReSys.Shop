Write-Host "Searching for any process on port 5000..."
$conns = Get-NetTCPConnection -LocalPort 5000 -ErrorAction SilentlyContinue
if ($conns) {
    $conns | Format-Table
} else {
    Write-Host "No NetTCPConnection found for 5000"
}

Write-Host "Searching for dotnet processes..."
Get-Process dotnet -ErrorAction SilentlyContinue | Select-Object Id, ProcessName, MainWindowTitle | Format-Table

Write-Host "Listing all listening ports..."
netstat -ano | findstr LISTENING | findstr :5000
