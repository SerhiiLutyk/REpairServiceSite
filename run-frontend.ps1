# Frontend start: install deps if missing, then run Vite dev server.
Set-Location $PSScriptRoot

if (-not (Test-Path "node_modules")) {
    Write-Host "Installing dependencies (npm install)..." -ForegroundColor Yellow
    npm install
}

if (-not (Test-Path ".env")) {
    Write-Host "Creating .env (VITE_API_URL=http://localhost:5038)..." -ForegroundColor Yellow
    "VITE_API_URL=http://localhost:5038" | Out-File -FilePath ".env" -Encoding ascii
}

Write-Host "Starting frontend (Vite)..." -ForegroundColor Green
npm run dev
