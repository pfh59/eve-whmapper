#!/bin/bash

# Require script to be run via sudo, but not as root
if [[ $EUID -ne 0 ]]; then
    echo "Script must be run with root privileges!"
    exit 1
elif [[ $EUID -eq $UID && -z "$SUDO_USER" ]]; then
    echo "Script must be run as current user via 'sudo', not as the root user!"
    exit 1
fi

set -euo pipefail

clear

echo "#------------------------------------------------------------#"
echo "EVE WHMAPPER DEPLOYMENT SCRIPT"
echo "#------------------------------------------------------------#"
echo

echo "Removing existing configuration..."
if [[ -d "./haproxy/certbot" ]]; then
    rm -rf ./haproxy/certbot/*
fi

if [[ -d "./haproxy/certs" ]]; then
    rm -rf ./haproxy/certs/*
fi

echo "#------------------------------------------------------------#"

echo "Checking prerequisites..."
if ! command -v docker &> /dev/null; then
    echo "Docker could not be found. Please install Docker and try again."
    exit 1
fi

echo "#------------------------------------------------------------#"

echo "Installation Wizard"
echo "Configuration Certbot and HAProxy"

while true; do
    read -rp "Enter your domain [Required]: " DOMAIN
    if [[ -n "$DOMAIN" && "$DOMAIN" =~ ^[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$ ]]; then
        break
    fi
    echo -e "\e[31mError: Domain must be set and valid (e.g., example.com)\e[0m"
done

while true; do
    read -rp "Enter your email (used by certbot to send an email before certificate expiration) [Required]: " EMAIL
    if [[ -n "$EMAIL" && "$EMAIL" =~ ^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$ ]]; then
        break
    fi
    echo -e "\e[31mError: A valid email address must be set (e.g., user@example.com)\e[0m"
done

read -rp 'Enter options (example: "-d sub1.your.domain.com") [OPTIONAL]: ' OPTIONS

echo
echo "Configure Db"
while true; do
    read -rs -p "Enter your root db password [Required]: " DBPASSWORD
    echo
    read -rs -p "Confirm your root db password [Required]: " CONFIRM_DBPASSWORD
    echo
    if [[ -z "$DBPASSWORD" ]]; then
        echo -e "\e[31mError: DB PASSWORD must be set\e[0m"
    elif [[ "$DBPASSWORD" != "$CONFIRM_DBPASSWORD" ]]; then
        echo -e "\e[31mError: Passwords do not match. Please try again.\e[0m"
    else
        break
    fi
done

echo
echo "Configuration CCP SSO"

while true; do
    read -rp "Enter CCP SSO ClientId [Required]: " SSOCLIENTID
    if [[ -n "$SSOCLIENTID" ]]; then
        break
    fi
    echo -e "\e[31mError: SSO CLIENTID must be set\e[0m"
done

while true; do
    read -rs -p "Enter CCP SSO Secret [Required]: " SSOSECRET
    echo
    read -rs -p "Confirm CCP SSO Secret [Required]: " CONFIRM_SSOSECRET
    echo
    if [[ -z "$SSOSECRET" ]]; then
        echo -e "\e[31mError: SSO SECRET must be set\e[0m"
    elif [[ "$SSOSECRET" != "$CONFIRM_SSOSECRET" ]]; then
        echo -e "\e[31mError: Secrets do not match. Please try again.\e[0m"
    else
        break
    fi
done

echo "#------------------------------------------------------------#"

# Check if all required files exist
if [[ ! -f "./haproxy/nginx/nginx.conf" ]]; then
    echo "Error: ./haproxy/nginx/nginx.conf not found"
    exit 1
fi

if [[ ! -f "./eve-whmapper/docker-compose.yml" ]]; then
    echo "Error: ./eve-whmapper/docker-compose.yml not found"
    exit 1
fi

echo "Applying configuration..."
readonly defaultDomain="mydomain.com"
sed -i "s|${defaultDomain}|${DOMAIN}|g" ./haproxy/nginx/nginx.conf

readonly defaultDbPwd1="POSTGRES_PASSWORD:-secret"
readonly defaultDbPwd2="Password=secret"

sed -i "s|${defaultDbPwd1}|POSTGRES_PASSWORD:-${DBPASSWORD}|g" ./eve-whmapper/docker-compose.yml
sed -i "s|${defaultDbPwd2}|Password=${DBPASSWORD}|g" ./eve-whmapper/docker-compose.yml

readonly defaultSSOClientId="EveSSO__ClientId=xxxxxxxxx"
sed -i "s|${defaultSSOClientId}|EveSSO__ClientId=${SSOCLIENTID}|g" ./eve-whmapper/docker-compose.yml

readonly defaultSSOSecret="EveSSO__Secret=xxxxxxxxx"
sed -i "s|${defaultSSOSecret}|EveSSO__Secret=${SSOSECRET}|g" ./eve-whmapper/docker-compose.yml

echo "Initializing..."

echo "Downloading images..."
docker compose -f ./haproxy/docker-compose.yml pull
docker compose -f ./eve-whmapper/docker-compose.yml pull

echo "Preparing containers..."
docker compose -f ./eve-whmapper/docker-compose.yml up --no-start
docker compose -f ./haproxy/docker-compose.yml up --no-start

echo "First start before creating HTTPS certs..."
docker compose -f ./haproxy/docker-compose.yml start

echo "Creating new certificate..."
docker compose -f ./haproxy/docker-compose.yml run --rm certbot certonly \
    --webroot --webroot-path /var/www/certbot/ \
    -d "$DOMAIN" --email "$EMAIL" \
    --non-interactive --agree-tos ${OPTIONS:+"$OPTIONS"}

# Merge private key and full chain in one file and add them to haproxy certs folder
cat_cert() {
    local dir="./haproxy/certbot/conf/live/${1}"
    cat "${dir}/privkey.pem" "${dir}/fullchain.pem" > "./haproxy/certs/${1}.pem"
}

echo "Run merge certificate for the requested domain name"
cat_cert "$DOMAIN"

echo "Configure HAProxy HTTPS binding"
readonly find_str="bind :443"
readonly replace_str="bind :443 ssl crt /usr/local/etc/certs/${DOMAIN}.pem alpn h2"
sed -i "s|${find_str}|${replace_str}|g" ./haproxy/conf/haproxy.cfg

echo "Restart HAProxy with HTTPS certs"
docker compose -f haproxy/docker-compose.yml kill -s HUP haproxy

echo "Stopping frontend HAProxy"
docker compose -f ./haproxy/docker-compose.yml stop

echo "Initialization complete."
