$proc = Get-Process dotnet -ErrorAction SilentlyContinue
if ($proc) {
    Stop-Process -InputObject $proc -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 1
    "Server stopped"
} else {
    "No dotnet processes running"
}
