param (
    [string]$DOMAIN
)
# Require script to be run as administrator
if (-not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Host "Script must be run with administrator privileges!"
    exit 1
}

#clear console
Clear-Host

Write-Host "#------------------------------------------------------------#"

Write-Host "Renewal certificates Wizard"
Write-Host "Configuration Certbot and HAProxy"
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

# Merge private key and full chain in one file and add them to haproxy certs folder
function cat-cert {
    param (
        [string]$domain
    )
    $dir = "./haproxy/certbot/conf/live/$domain"
    Get-Content "$dir/privkey.pem", "$dir/fullchain.pem" | Set-Content "./haproxy/certs/$domain.pem"
}

Write-Host "Simulate the certificate renewal for the requested domain name"

if($IsWindows)
{
    docker-compose -f .\haproxy\docker-compose.yml run --rm certbot --dry-run
}
else
{
    docker-compose -f ./haproxy/docker-compose.yml run --rm certbot --dry-run
}

#check test is ok
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Certificate renewal failed" -ForegroundColor Red
    exit 1
}

Write-Host "Renew the certificate for the requested domain name"
if($IsWindows)
{
    docker-compose -f .\haproxy\docker-compose.yml run --rm certbot renew --dry-run
}
else
{
    docker-compose -f ./haproxy/docker-compose.yml run --rm certbot renew --dry-run
}

#checc if renew is ok
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Certificate renewal failed" -ForegroundColor Red
    exit 1
}

Write-Host "Run merge certificate for the requested domain name"
cat-cert -domain $DOMAIN
