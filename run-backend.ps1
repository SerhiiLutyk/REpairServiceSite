# Reliable backend start: kill stale processes, ensure Docker, run Aspire.
Write-Host "Stopping stale processes (dotnet, GadgetFix*)..." -ForegroundColor Yellow
Get-Process dotnet, 'GadgetFix*' -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 3

# Check / start Docker
docker ps *> $null
if ($LASTEXITCODE -ne 0) {
    Write-Host "Docker is not running - starting Docker Desktop..." -ForegroundColor Yellow
    Start-Process "C:\Program Files\Docker\Docker\Docker Desktop.exe"
    Write-Host "Waiting for Docker (up to 2 min)..." -ForegroundColor Yellow
    $ready = $false
    for ($i = 0; $i -lt 40; $i++) {
        Start-Sleep -Seconds 3
        docker ps *> $null
        if ($LASTEXITCODE -eq 0) { $ready = $true; break }
    }
    if (-not $ready) {
        Write-Host "Docker did not start. Launch Docker Desktop manually and retry." -ForegroundColor Red
        exit 1
    }
}
Write-Host "Docker is ready." -ForegroundColor Green

Write-Host "Starting backend (Aspire)..." -ForegroundColor Green
Set-Location $PSScriptRoot\backend
dotnet run --project GadgetFix.AppHost
