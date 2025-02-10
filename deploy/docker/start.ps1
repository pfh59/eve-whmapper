# Require script to be run via elevated privileges, but not as Administrator

if (-not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Host "Script must be run with elevated privileges!"
    exit 1
}

# Clear console
Clear-Host

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


Write-Host "Starting..."
Write-Host "Starting backend..."
if($IsWindows) {
    docker-compose -f .\eve-whmapper\docker-compose.yml start
} else {
    docker-compose -f ./eve-whmapper/docker-compose.yml start
}


Write-Host "Starting frontend..."
if($IsWindows) {
    docker-compose -f .\haproxy\docker-compose.yml start
} else {
    docker-compose -f ./haproxy/docker-compose.yml start
}