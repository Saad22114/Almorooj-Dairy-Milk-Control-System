#!/usr/bin/env pwsh

Write-Host "Farmers App - ASP.NET Core" -ForegroundColor Green
Write-Host "==========================" -ForegroundColor Green
Write-Host ""

Set-Location -Path $PSScriptRoot

# Check if dotnet is installed
$dotnetPath = Get-Command dotnet -ErrorAction SilentlyContinue
if (-not $dotnetPath) {
    Write-Host "[ERROR] .NET SDK is not installed or not in PATH" -ForegroundColor Red
    Write-Host "Please install .NET 8.0 SDK from https://dotnet.microsoft.com/download" -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "Building and starting the application..." -ForegroundColor Cyan
Write-Host ""

$serverProcess = Start-Process -FilePath "dotnet" -ArgumentList @("run", "--urls", "http://0.0.0.0:5000") -PassThru

Write-Host "Waiting for server to become ready on http://localhost:5000 ..." -ForegroundColor Cyan
for ($i = 0; $i -lt 30; $i++) {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5000" -UseBasicParsing -TimeoutSec 2
        if ($response.StatusCode -ge 200 -and $response.StatusCode -lt 500) { break }
    } catch {
        Start-Sleep -Seconds 1
    }
}

Start-Process "http://localhost:5000"
Write-Host "Server started. Browser opened at http://localhost:5000" -ForegroundColor Green
Write-Host "To stop the server, close this window or stop dotnet process." -ForegroundColor Yellow

Wait-Process -Id $serverProcess.Id
