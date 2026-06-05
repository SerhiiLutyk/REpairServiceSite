# Надійний запуск бекенду: спершу прибирає старі процеси, тоді запускає Aspire.
Write-Host "Зупиняю старі процеси (dotnet, GadgetFix*)..." -ForegroundColor Yellow
Get-Process dotnet, 'GadgetFix*' -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 3

# перевірка Docker
docker ps *> $null
if ($LASTEXITCODE -ne 0) {
    Write-Host "Docker не запущено. Увімкни Docker Desktop і повтори." -ForegroundColor Red
    exit 1
}

Write-Host "Запускаю бекенд (Aspire)..." -ForegroundColor Green
Set-Location $PSScriptRoot\backend
dotnet run --project GadgetFix.AppHost
