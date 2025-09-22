param (
    [string]$DOMAIN,
    [string]$EMAIL,
    [string]$OPTIONS,
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

# Clear console
Write-Host "#------------------------------------------------------------#"
Write-Host "EVE WHMAPPER DEPLOYMENT SCRIPT"
Write-Host "#------------------------------------------------------------#"

Write-Host "Removing existing configuration..."
#remove if exists
if (Test-Path .\haproxy\certbot\conf) {
    Remove-Item -Recurse -Force .\haproxy\certbot\*
}
if (Test-Path .\haproxy\certs) {
    Remove-Item -Recurse -Force .\haproxy\certs\*
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
Write-Host "#------------------------------------------------------------#"

Write-Host "Installation Wizard"
Write-Host "Configuring Certbot and HAProxy"

if (-not $DOMAIN) {
    do {
        $DOMAIN = Read-Host "Enter your domain [Required]"
        if (-not $DOMAIN) {
            Write-Host "Error: Domain must be set" -ForegroundColor Red
        } elseif (-not ($DOMAIN -match "^[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")) {
            Write-Host "Error: Domain is not valid" -ForegroundColor Red
            $DOMAIN = $null
        }
    } while (-not $DOMAIN)
}

if (-not $EMAIL) {
    do {
        $EMAIL = Read-Host "Enter your email (used by certbot to send an email before certificate expiration) [Required]"
        if (-not $EMAIL) {
            Write-Host "Error: EMAIL must be set" -ForegroundColor Red
        } elseif (-not ($EMAIL -match "^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")) {
            Write-Host "Error: Email is not valid" -ForegroundColor Red
            $EMAIL = $null
        }
    } while (-not $EMAIL)
}

if (-not $OPTIONS) {
    $OPTIONS = Read-Host "Enter options (example: '-d sub1.your.domain.com') [OPTIONAL]"
}

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
        if ($PSVersionTable.PSEdition -eq "Desktop") {
            $plainDbPassword1 = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($DBPASSWORD))
            $plainDbPassword2 = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($DBPASSWORD2))
        } elseif ($PSVersionTable.PSEdition -eq "Core") {
            $plainDbPassword1 = ConvertFrom-SecureString -SecureString $DBPASSWORD -AsPlainText
            $plainDbPassword2 = ConvertFrom-SecureString -SecureString $DBPASSWORD2 -AsPlainText
        }

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
            if ($PSVersionTable.PSEdition -eq "Desktop") {
                $plainSSOSECRET1 = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($SSOSECRET))
                $plainSSOSECRET2 = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($SSOSECRET2))
            } elseif ($PSVersionTable.PSEdition -eq "Core") {
                $plainSSOSECRET1 = ConvertFrom-SecureString -SecureString $SSOSECRET -AsPlainText
                $plainSSOSECRET2 = ConvertFrom-SecureString -SecureString $SSOSECRET2 -AsPlainText
            }

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
    ".\haproxy\nginx\nginx.conf",
    ".\eve-whmapper\docker-compose.yml"
)

foreach ($file in $requiredFiles) {
    if (-not (Test-Path $file)) {
        Write-Host "Error: Required file $file does not exist" -ForegroundColor Red
        exit 1
    }
}

Write-Host "Applying configuration..."
$defaultDomain = "mydomain.com"
(Get-Content .\haproxy\nginx\nginx.conf) -replace $defaultDomain, $DOMAIN | Set-Content .\haproxy\nginx\nginx.conf

if ($PSVersionTable.PSEdition -eq "Desktop") {
    $plainDbPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($DBPASSWORD))
} elseif ($PSVersionTable.PSEdition -eq "Core") {
    $plainDbPassword = ConvertFrom-SecureString -SecureString $DBPASSWORD -AsPlainText
}
$defaultDbPwd1 = "POSTGRES_PASSWORD:-secret"
$defaultDbPwd2 = "Password=secret"
$defaultSSOClientId = "EveSSO__ClientId=xxxxxxxxx"
$defaultSSOSecret = "EveSSO__Secret=xxxxxxxxx"
if ($PSVersionTable.PSEdition -eq "Desktop") {
    $plainSSOSECRET = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($SSOSECRET))
} elseif ($PSVersionTable.PSEdition -eq "Core") {
    $plainSSOSECRET = ConvertFrom-SecureString -SecureString $SSOSECRET -AsPlainText
}


if($IsWindows) {
    (Get-Content .\eve-whmapper\docker-compose.yml) -replace $defaultDbPwd1, "POSTGRES_PASSWORD:-$plainDbPassword" | Set-Content .\eve-whmapper\docker-compose.yml
    (Get-Content .\eve-whmapper\docker-compose.yml) -replace $defaultDbPwd2, "Password=$plainDbPassword" | Set-Content .\eve-whmapper\docker-compose.yml
    (Get-Content .\eve-whmapper\docker-compose.yml) -replace $defaultSSOClientId, "EveSSO__ClientId=$SSOCLIENTID" | Set-Content .\eve-whmapper\docker-compose.yml
    (Get-Content .\eve-whmapper\docker-compose.yml) -replace $defaultSSOSecret, "EveSSO__Secret=$plainSSOSECRET" | Set-Content .\eve-whmapper\docker-compose.yml

}
else {
    (Get-Content ./eve-whmapper/docker-compose.yml) -replace $defaultDbPwd1, "POSTGRES_PASSWORD:-$plainDbPassword" | Set-Content ./eve-whmapper/docker-compose.yml
    (Get-Content ./eve-whmapper/docker-compose.yml) -replace $defaultDbPwd2, "Password=$plainDbPassword" | Set-Content ./eve-whmapper/docker-compose.yml
    (Get-Content ./eve-whmapper/docker-compose.yml) -replace $defaultSSOClientId, "EveSSO__ClientId=$SSOCLIENTID" | Set-Content ./eve-whmapper/docker-compose.yml
    (Get-Content ./eve-whmapper/docker-compose.yml) -replace $defaultSSOSecret, "EveSSO__Secret=$plainSSOSECRET" | Set-Content ./eve-whmapper/docker-compose.yml
}


Write-Host "Initializing..."

Write-Host "Download Images ..."
if ($IsWindows) {
    docker-compose -f .\haproxy\docker-compose.yml pull
    docker-compose -f .\eve-whmapper\docker-compose.yml pull
} else {
    docker-compose -f ./haproxy/docker-compose.yml pull
    docker-compose -f ./eve-whmapper/docker-compose.yml pull
}

Write-Host "Prepare container"
if ($IsWindows) {
    docker-compose -f .\eve-whmapper\docker-compose.yml up --no-start
    docker-compose -f .\haproxy\docker-compose.yml up --no-start
} else {
    docker-compose -f ./eve-whmapper/docker-compose.yml up --no-start
    docker-compose -f ./haproxy/docker-compose.yml up --no-start
}


Write-Host "First start before creating HTTPS certs..."
if ($IsWindows) {
    docker-compose -f .\haproxy\docker-compose.yml start
} else {
    docker-compose -f ./haproxy/docker-compose.yml start
}

Write-Host "Starting create new certificate..."
if($IsWindows) {
    docker-compose -f .\haproxy\docker-compose.yml run --rm certbot certonly --webroot --webroot-path /var/www/certbot/ -d $DOMAIN --email $EMAIL --non-interactive --agree-tos $OPTIONS
}
else {
    docker-compose -f ./haproxy/docker-compose.yml run --rm certbot certonly --webroot --webroot-path /var/www/certbot/ -d $DOMAIN --email $EMAIL --non-interactive --agree-tos $OPTIONS
}

# Merge private key and full chain in one file and add them to haproxy certs folder
function cat-cert {
    param (
        [string]$domain
    )
    if($IsWindows) {
        $dir = ".\haproxy\certbot\conf\live\$domain"
        Get-Content "$dir\privkey.pem", "$dir\fullchain.pem" | Set-Content ".\haproxy\certs\$domain.pem"
    }
    else {
        $dir = "./haproxy/certbot/conf/live/$domain"
        Get-Content "$dir/privkey.pem", "$dir/fullchain.pem" | Set-Content "./haproxy/certs/$domain.pem"
    }
}

Write-Host "Run merge certificate for the requested domain name"
cat-cert -domain $DOMAIN

Write-Host "Configure HAProxy HTTPS binding"
$find = "bind :443"
$replace = "bind :443 ssl crt /usr/local/etc/certs/$DOMAIN.pem alpn h2"
if($IsWindows) {
    (Get-Content .\haproxy\conf\haproxy.cfg) -replace $find, $replace | Set-Content .\haproxy\conf\haproxy.cfg
}
else {
    (Get-Content ./haproxy/conf/haproxy.cfg) -replace $find, $replace | Set-Content ./haproxy/conf/haproxy.cfg
}


Write-Host "Restart HAPROXY with HTTPS certs"
if($IsWindows) {
    docker-compose -f .\haproxy\docker-compose.yml kill -s HUP haproxy
}
else {
    docker-compose -f ./haproxy/docker-compose.yml kill -s HUP haproxy
}

Write-Host "Stopping frontend HAPROXY"
if($IsWindows) {
    docker-compose -f .\haproxy\docker-compose.yml stop
}
else {
    docker-compose -f ./haproxy/docker-compose.yml stop
}
