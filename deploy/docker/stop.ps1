# Require script to be run via sudo, but not as root

if (-not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Host "Script must be run with administrator privileges!"
    exit 1
}
# clear console
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

Write-Host "Stopping..."
if($IsWindows) {
    Write-Host "Stopping backend..."
    docker-compose -f .\eve-whmapper\docker-compose.yml stop
    Write-Host "Stopping frontend..."
    docker-compose -f .\haproxy\docker-compose.yml stop
} else {
    Write-Host "Stopping backend..."
    docker-compose -f ./eve-whmapper/docker-compose.yml stop
    Write-Host "Stopping frontend..."
    docker-compose -f ./haproxy/docker-compose.yml stop
}

