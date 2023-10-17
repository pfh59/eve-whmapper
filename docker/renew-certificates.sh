#!/bin/bash

clear

# Require script to be run via sudo, but not as root
if [[ $EUID != 0 ]]; then
    echo "Script must be run with root privilages!"
    exit 1
elif [[ $EUID = $UID && "$SUDO_USER" = "" ]]; then
    echo "Script must be run as current user via 'sudo', not as the root user!"
    exit 1
fi

set -e

echo "#------------------------------------------------------------#"

echo "Rnewal certificates  Wizard"
echo "Configation Certbot and HAProxy"
read -p "Enter you domain [Required] :" DOMAIN
if [ -z "$DOMAIN" ]
then
      echo "Error: Domain must be set"
      exit 1
fi

# Merge private key and full chain in one file and add them to haproxy certs folder
function cat-cert() {
  dir="./haproxy/certbot/conf/live/$1"
  cat "$dir/privkey.pem" "$dir/fullchain.pem" > "./haproxy/certs/$1.pem"
}

echo "Starting create new certificate..."
docker-compose -f ./haproxy/docker-compose.yml run --rm certbot renew

echo "Run merge certificate for the requested domain name"
cat-cert $DOMAIN

echo "Verify the certificat renewed"
docker-compose -f ./haproxy/docker-compose.yml run --rm certbot renew --dry-run