# ReSys.Shop Integration & Unit Test Runner

Write-Host "--- ReSys.Shop Test Runner ---" -ForegroundColor Cyan

$projects = @(
    "tests/ReSys.Core.UnitTests/ReSys.Core.UnitTests.csproj",
    "tests/ReSys.Api.UnitTests/ReSys.Api.UnitTests.csproj",
    "tests/ReSys.Infrastructure.UnitTests/ReSys.Infrastructure.UnitTests.csproj",
    "tests/ReSys.Identity.IntegrationTests/ReSys.Identity.IntegrationTests.csproj",
    "tests/ReSys.Api.IntegrationTests/ReSys.Api.IntegrationTests.csproj"
)

$failed = @()

foreach ($project in $projects) {
    Write-Host "`n>>> Running tests for: $project" -ForegroundColor Yellow
    dotnet test $project --configuration Release
    if ($LASTEXITCODE -ne 0) {
        $failed += $project
    }
}

Write-Host "`n------------------------------------" -ForegroundColor Gray
if ($failed.Count -eq 0) {
    Write-Host "ALL TESTS PASSED! ✅" -ForegroundColor Green
} else {
    Write-Host "SOME TESTS FAILED: ❌" -ForegroundColor Red
    foreach ($f in $failed) {
        Write-Host "  - $f" -ForegroundColor Red
    }
    exit 1
}
