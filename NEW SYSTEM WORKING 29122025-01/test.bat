@echo off
REM FarmersApp Test Script

echo ============================================================
echo FarmersApp - Project Verification and Testing
echo ============================================================
echo.

REM Step 1: Check .NET SDK
echo [1/5] Checking .NET SDK...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: .NET SDK not found
    exit /b 1
) else (
    echo OK: .NET SDK found
)

REM Step 2: Check project structure
echo.
echo [2/5] Checking project structure...
if exist "Controllers" (echo   OK: Controllers directory found) else (echo   ERROR: Controllers directory missing & exit /b 1)
if exist "Models" (echo   OK: Models directory found) else (echo   ERROR: Models directory missing & exit /b 1)
if exist "Services" (echo   OK: Services directory found) else (echo   ERROR: Services directory missing & exit /b 1)
if exist "Views" (echo   OK: Views directory found) else (echo   ERROR: Views directory missing & exit /b 1)
if exist "Program.cs" (echo   OK: Program.cs found) else (echo   ERROR: Program.cs missing & exit /b 1)
if exist "FarmersApp.csproj" (echo   OK: FarmersApp.csproj found) else (echo   ERROR: FarmersApp.csproj missing & exit /b 1)

REM Step 3: Build project
echo.
echo [3/5] Building project...
dotnet clean --quiet >nul 2>&1
dotnet restore --quiet >nul 2>&1
dotnet build --no-restore --quiet >nul 2>&1
if errorlevel 1 (
    echo ERROR: Build failed
    exit /b 1
) else (
    echo OK: Build successful
)

REM Step 4: Check output
echo.
echo [4/5] Checking build output...
if exist "bin\Debug\net8.0\FarmersApp.dll" (
    echo OK: FarmersApp.dll generated
) else (
    echo ERROR: FarmersApp.dll not found
    exit /b 1
)

REM Step 5: Summary
echo.
echo ============================================================
echo ALL TESTS PASSED - Project is ready!
echo ============================================================
echo.
echo To run the application:
echo   Command: dotnet run
echo   Browser: http://localhost:5000
echo.
pause
