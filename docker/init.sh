#!/bin/bash

# Require script to be run via sudo, but not as root

if [[ $EUID != 0 ]]; then
    echo "Script must be run with root privilages!"
    exit 1
elif [[ $EUID = $UID && "$SUDO_USER" = "" ]]; then
    echo "Script must be run as current user via 'sudo', not as the root user!"
    exit 1
fi

set -e

if [ "$#" -lt 2 ]; then
    echo "Usage: ...  <domain> <email> [options]"
    exit
fi

DOMAIN=$1
EMAIL=$2
OPTIONS=$3

echo "Cleaning"
rm -rf ./haproxy/certbot/*
rm -rf ./haproxy/certs/*

echo "Configuration certbot-nginx domain" 
defaultDomain="mydomain.com"
sed -i "s|$defaultDomain|$DOMAIN|g" ./haproxy/nginx/nginx.conf

echo "Initializing..."

echo "Download Images ..."
docker-compose -f ./haproxy/docker-compose.yml pull
docker-compose -f ./eve-whmapper/docker-compose.yml pull

echo "Prepare container"
docker-compose -f ./haproxy/docker-compose.yml up --no-start
docker-compose -f ./eve-whmapper/docker-compose.yml up --no-start

echo "First start before cratingt HTTPS certs..."
docker-compose -f ./haproxy/docker-compose.yml start

echo "Starting create new certificate..."

docker-compose -f ./haproxy/docker-compose.yml run --rm  certbot certonly --webroot --webroot-path /var/www/certbot/ -d $DOMAIN --email $EMAIL --non-interactive --agree-tos $3

# Merge private key and full chain in one file and add them to haproxy certs folder
function cat-cert() {
  dir="./haproxy/certbot/conf/live/$1"
  cat "$dir/privkey.pem" "$dir/fullchain.pem" > "./haproxy/certs/$1.pem"
}

echo "Run merge certificate for the requested domain name"
cat-cert $DOMAIN


echo "Configura HAProxy HTTPS binding"
find="bind :443"
replace="bind :443 ssl crt /usr/local/etc/certs/$1.pem alpn h2"
sed -i "s|$find|$replace|g" ./haproxy/conf/haproxy.cfg

echo "Restart HAPROXY with HTTPS certs"
docker-compose -f haproxy/docker-compose.yml kill -s HUP haproxy


echo "Stoping frontend HAPROXY"
docker-compose -f ./haproxy/docker-compose.yml stop
