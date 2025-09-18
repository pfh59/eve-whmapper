# Require script to be run via elevated privileges, but not as Administrator

if (-not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Host "Script must be run with elevated privileges!"
    exit 1
}

# Clear console
Clear-Host

# change script directory as working directory
Set-Location -Path $PSScriptRoot


# check prerequisites
$docker = Get-Command -Name "docker" -ErrorAction SilentlyContinue
$dockerCompose = Get-Command -Name "docker-compose" -ErrorAction SilentlyContinue

if (-not $docker) {
    Write-Host "Error: Docker is not installed" -ForegroundColor Red
    exit 1
}

if (-not $dockerCompose) {
    Write-Host "Error: Docker Compose is not installed" -ForegroundColor Red
    exit 1
}


Write-Host "Stopping..."
docker compose stop

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Docker compose failed to stop containers" -ForegroundColor Red
    exit 1
}

Write-Host "Done."


