# Надійний запуск бекенду: прибирає старі процеси, вмикає Docker за потреби, запускає Aspire.
Write-Host "Зупиняю старі процеси (dotnet, GadgetFix*)..." -ForegroundColor Yellow
Get-Process dotnet, 'GadgetFix*' -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 3

# Перевірка / запуск Docker
docker ps *> $null
if ($LASTEXITCODE -ne 0) {
    Write-Host "Docker не запущено — вмикаю Docker Desktop..." -ForegroundColor Yellow
    Start-Process "C:\Program Files\Docker\Docker\Docker Desktop.exe"
    Write-Host "Чекаю готовності Docker (до 2 хв)..." -ForegroundColor Yellow
    $ready = $false
    for ($i = 0; $i -lt 40; $i++) {
        Start-Sleep -Seconds 3
        docker ps *> $null
        if ($LASTEXITCODE -eq 0) { $ready = $true; break }
    }
    if (-not $ready) {
        Write-Host "Docker не піднявся. Запусти Docker Desktop вручну й повтори." -ForegroundColor Red
        exit 1
    }
}
Write-Host "Docker готовий." -ForegroundColor Green

Write-Host "Запускаю бекенд (Aspire)..." -ForegroundColor Green
Set-Location $PSScriptRoot\backend
dotnet run --project GadgetFix.AppHost
