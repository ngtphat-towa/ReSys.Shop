$ports = @(5074, 8000, 5000, 5001, 7073, 5173, 5174, 7217)
foreach ($p in $ports) {
    $conns = Get-NetTCPConnection -LocalPort $p -ErrorAction SilentlyContinue
    if ($conns) {
        foreach ($c in $conns) {
            $proc = Get-Process -Id $c.OwningProcess -ErrorAction SilentlyContinue
            Write-Host "Port $p is used by PID $($c.OwningProcess) ($($proc.ProcessName))"
        }
    } else {
        Write-Host "Port $p is FREE"
    }
}
