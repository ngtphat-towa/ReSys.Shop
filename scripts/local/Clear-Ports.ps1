Write-Host "Cleaning up ports..."
$ports = @(5000, 5001, 7073, 5074, 7217, 8000, 5173, 5174)
foreach ($p in $ports) {
    $conns = Get-NetTCPConnection -LocalPort $p -ErrorAction SilentlyContinue
    if ($conns) {
        foreach ($c in $conns) {
            try {
                Stop-Process -Id $c.OwningProcess -Force -ErrorAction SilentlyContinue
                Write-Host "Stopped process $($c.OwningProcess) on port $p"
            } catch {}
        }
    }
}
Write-Host "Removing DB container..."
docker rm -f resys_shop_db