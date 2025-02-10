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
#clear screen
clear

echo "#------------------------------------------------------------#"

echo "Rnewal certificates  Wizard"
echo "Configation Certbot and HAProxy"
while true; do
  read -p "Enter your domain [Required]: " DOMAIN
  if [[ -z "$DOMAIN" ]]; then
    echo -e "\e[31mError: Domain must be set\e[0m"
  elif ! [[ "$DOMAIN" =~ ^[a-zA-Z0-9.-]+$ ]]; then
    echo -e "\e[31mError: Invalid domain format\e[0m"
  else
    break
  fi
done

# Merge private key and full chain in one file and add them to haproxy certs folder
function cat-cert() {
  dir="./haproxy/certbot/conf/live/$DOMAIN"
  cat "$dir/privkey.pem" "$dir/fullchain.pem" > "./haproxy/certs/$DOMAIN.pem"
}

echo "Simulate the certificate renewal for the requested domain name"
docker-compose -f ./haproxy/docker-compose.yml run --rm certbot renew --dry-run

#check if test if ok
if [ $? -ne 0 ]; then
  echo -e "\e[31mError: Certificate renewal failed\e[0m"
  exit 1
fi

echo "Renew the certificate for the requested domain name"
docker-compose -f ./haproxy/docker-compose.yml run --rm certbot renew

#check if renew is ok
if [ $? -ne 0 ]; then
  echo -e "\e[31mError: Certificate renewal failed\e[0m"
  exit 1
fi

echo "Run merge certificate for the requested domain name"
cat-cert $DOMAIN

