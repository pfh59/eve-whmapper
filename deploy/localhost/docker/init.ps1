param (
    [securestring]$DBPASSWORD,
    [string]$SSOCLIENTID,
    [securestring]$SSOSECRET
)


# Require script to be run as administrator
if (-not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Host "Script must be run with administrator privileges!"
    exit 1
}

Clear-Host

# change script directory as working directory
Set-Location -Path $PSScriptRoot




# Clear console
Write-Host "#------------------------------------------------------------#"
Write-Host "EVE WHMAPPER DEPLOYMENT SCRIPT"
Write-Host "#------------------------------------------------------------#"

Write-Host "Removing existing configuration..."
docker compose down -v
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Docker compose failed to stop existing containers" -ForegroundColor Red
    exit 1
}

docker compose rm -f
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Docker compose failed to remove existing containers" -ForegroundColor Red
    exit 1
}

#remove if exists
if (Test-Path .\nginx\certs) {
    Remove-Item -Recurse -Force .\nginx\certs
}

Write-Host "#------------------------------------------------------------#"

Write-Host "Checking requirements..."
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

#Check is mkcert is installed
$mkcert = Get-Command -Name "mkcert" -ErrorAction SilentlyContinue
if (-not $mkcert) {
    Write-Host "Error: mkcert is not installed. Please install mkcert from https://github.com/FiloSottile/mkcert" -ForegroundColor Red
    exit 1
}

Write-Host "#------------------------------------------------------------#"

Write-Host "Installation Wizard"


Write-Host "Create self-signed certificate for local deployment whmapper.local"
& $mkcert -install
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: mkcert failed to install local CA" -ForegroundColor Red
    exit 1
}
#Create cert for whmapper.local on nginx certs folder
if (-not (Test-Path .\nginx\certs)) {
    New-Item -ItemType Directory -Path .\nginx\certs
}
& $mkcert -cert-file whmapper.local.pem -key-file whmapper.local.key whmapper.local 127.0.0.1 ::1

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: mkcert failed to create certificate" -ForegroundColor Red
    exit 1
}
# move to nginx certs folder
Move-Item -Path .\whmapper.local.pem -Destination .\nginx\certs
Move-Item -Path .\whmapper.local.key -Destination .\nginx\certs

Write-Host "`n"
Write-Host "Configuring Db"
if (-not $DBPASSWORD) {
    do {
        $DBPASSWORD = Read-Host -AsSecureString "Enter your root db password [Required]"
        if (-not $DBPASSWORD) {
            Write-Host "Error: DB PASSWORD must be set" -ForegroundColor Red
            exit 1
        }

        # Confirm password
        $DBPASSWORD2 = Read-Host -AsSecureString "Confirm your root db password [Required]"
        $plainDbPassword1 = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($DBPASSWORD))
        $plainDbPassword2 = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($DBPASSWORD2))
        
        if ($plainDbPassword1 -ne $plainDbPassword2) {
            Write-Host "Error: Passwords do not match. Please try again." -ForegroundColor Red
        }
    } while ($plainDbPassword1 -ne $plainDbPassword2)
}

Write-Host "`n"
Write-Host "Configuration CCP SSO"

if (-not $SSOCLIENTID) {
    do {
        $SSOCLIENTID = Read-Host "Enter CCP SSO ClientId [Required]"
        if (-not $SSOCLIENTID) {
            Write-Host "Error: SSO CLIENTID must be set" -ForegroundColor Red
        }
    } while (-not $SSOCLIENTID)
}

if (-not $SSOSECRET) {
    do {
        $SSOSECRET = Read-Host -AsSecureString "Enter CCP SSO Secret [Required]"
        if (-not $SSOSECRET) {
            Write-Host "Error: SSO SECRET must be set"
        } else {
            # Confirm secret
            $SSOSECRET2 = Read-Host -AsSecureString "Confirm CCP SSO Secret [Required]"
            $plainSSOSECRET1 = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($SSOSECRET))
            $plainSSOSECRET2 = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($SSOSECRET2))

            if ($plainSSOSECRET1 -ne $plainSSOSECRET2) {
                Write-Host "Error: Secrets do not match. Please try again." -ForegroundColor Red
                $SSOSECRET = $null
            }
        }
    } while (-not $SSOSECRET)
}

Write-Host "#------------------------------------------------------------#"

Write-Host "Checking if all required files exist..."

$requiredFiles = @(
    ".\nginx\conf\nginx.conf",
    ".\nginx\certs\whmapper.local.pem",
    ".\nginx\certs\whmapper.local.key",
    ".\docker-compose.yml"
)

foreach ($file in $requiredFiles) {
    if (-not (Test-Path $file)) {
        Write-Host "Error: Required file $file does not exist" -ForegroundColor Red
        exit 1
    }
}

$plainDbPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($DBPASSWORD))
$defaultDbPwd1 = "POSTGRES_PASSWORD:-secret"
$defaultDbPwd2 = "Password=secret"
$defaultSSOClientId = "EveSSO__ClientId=xxxxxxxxx"
$defaultSSOSecret = "EveSSO__Secret=xxxxxxxxx"
$plainSSOSECRET = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($SSOSECRET))


if($IsWindows) {
    (Get-Content .\docker-compose.yml) -replace $defaultDbPwd1, "POSTGRES_PASSWORD:-$plainDbPassword" | Set-Content .\docker-compose.yml
    (Get-Content .\docker-compose.yml) -replace $defaultDbPwd2, "Password=$plainDbPassword" | Set-Content .\docker-compose.yml
    (Get-Content .\docker-compose.yml) -replace $defaultSSOClientId, "EveSSO__ClientId=$SSOCLIENTID" | Set-Content .\docker-compose.yml
    (Get-Content .\docker-compose.yml) -replace $defaultSSOSecret, "EveSSO__Secret=$plainSSOSECRET" | Set-Content .\docker-compose.yml

}
else {
    (Get-Content ./docker-compose.yml) -replace $defaultDbPwd1, "POSTGRES_PASSWORD:-$plainDbPassword" | Set-Content ./docker-compose.yml
    (Get-Content ./docker-compose.yml) -replace $defaultDbPwd2, "Password=$plainDbPassword" | Set-Content ./docker-compose.yml
    (Get-Content ./docker-compose.yml) -replace $defaultSSOClientId, "EveSSO__ClientId=$SSOCLIENTID" | Set-Content ./docker-compose.yml
    (Get-Content ./docker-compose.yml) -replace $defaultSSOSecret, "EveSSO__Secret=$plainSSOSECRET" | Set-Content ./docker-compose.yml
}


Write-Host "Initializing..."

Write-Host "Download Images ..."
docker compose pull
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Docker compose failed to pull images" -ForegroundColor Red
    exit 1
}


Write-Host "Prepare container"
docker compose  up --no-start
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Docker compose failed to prepare containers" -ForegroundColor Red
    exit 1
}

