$workDir = "c:\DATA NEW SYSTEM  10%\DATA NEW SYSTEM 10%"
$proc = Start-Process -FilePath "C:\Program Files\dotnet\dotnet.exe" -ArgumentList "run --configuration Release --urls http://0.0.0.0:5000" -WorkingDirectory $workDir -PassThru -WindowStyle Hidden
Write-Host "Server started with PID: $($proc.Id)"
Start-Sleep -Seconds 3
Write-Host "Server should be running"
