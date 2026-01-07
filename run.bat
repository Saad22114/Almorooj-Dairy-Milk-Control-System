@echo off
REM Farmers App Launcher - ASP.NET Core

echo Farmers App - ASP.NET Core
echo ==========================
echo.

REM Check if dotnet is installed
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo [ERROR] .NET SDK is not installed or not in PATH
    echo Please install .NET 8.0 SDK from https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo Building and starting the application...
echo.

dotnet run

pause
