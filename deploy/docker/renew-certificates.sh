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

echo "Renewal certificates Wizard"
echo "Configuration Certbot and HAProxy"
while true; do
    read -rp "Enter your domain [Required]: " DOMAIN
    if [[ -z "$DOMAIN" ]]; then
        echo -e "\e[31mError: Domain must be set\e[0m"
    elif ! [[ "$DOMAIN" =~ ^[a-zA-Z0-9.-]+$ ]]; then
        echo -e "\e[31mError: Invalid domain format\e[0m"
    else
        break
    fi
done

# Merge private key and full chain in one file and add them to haproxy certs folder
cat_cert() {
    local dir="./haproxy/certbot/conf/live/${1}"
    cat "${dir}/privkey.pem" "${dir}/fullchain.pem" > "./haproxy/certs/${1}.pem"
}

echo "Simulating certificate renewal for the requested domain name..."
if ! docker compose -f ./haproxy/docker-compose.yml run --rm certbot renew --dry-run; then
    echo -e "\e[31mError: Certificate renewal dry-run failed\e[0m"
    exit 1
fi

echo "Renewing certificate for the requested domain name..."
if ! docker compose -f ./haproxy/docker-compose.yml run --rm certbot renew; then
    echo -e "\e[31mError: Certificate renewal failed\e[0m"
    exit 1
fi

echo "Run merge certificate for the requested domain name"
cat_cert "$DOMAIN"

echo "Certificate renewal complete."

