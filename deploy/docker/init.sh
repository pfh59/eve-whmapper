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

echo "Cleaning cert directories"
rm -rf ./haproxy/certbot/*
rm -rf ./haproxy/certs/*

echo "#------------------------------------------------------------#"

echo "Installation Wizard"
echo "Configation Certbot and HAProxy"
read -p "Enter you domain [Required] :" DOMAIN
if [ -z "$DOMAIN" ]
then
      echo "Error: Domain must be set"
      exit 1
fi

read -p "Enter your email (use by certbot to send a email before certificat expiration.) [Required] :" EMAIL
if [ -z "$EMAIL" ]
then
      echo "Error: EMAIL must be set"
      exit 1
fi

read -p 'Enter options (exemple : "-d sub1.your.doamin.com") [OPTIONAL] :' OPTIONS

echo "Configurate Db"
read -s -p "Enter your root db password [Required] :" DBPASSWORD
if [ -z "$DBPASSWORD" ]
then
      echo "Error: DB PASSWORD must be set"
      exit 1
fi

echo  "\n"
echo "Configuration CCP SSO"

read -p "Enter CCP SSO ClientId [Required] :" SSOCLIENTID
if [ -z "$SSOCLIENTID" ]
then
      echo "Error: SSO CLIENTID must be set"
      exit 1
fi

read -p "Enter CCP SSO Secret [Required] :" SSOSECRET
if [ -z "$SSOSECRET" ]
then
      echo "Error: SSO SECRET must be set"
      exit 1
fi

echo "#------------------------------------------------------------#"
echo "Applying configuration..." 
defaultDomain="mydomain.com"
sed -i "s|$defaultDomain|$DOMAIN|g" ./haproxy/nginx/nginx.conf

defaultDbPwd1="POSTGRES_PASSWORD:-secret"
defaultDbPwd2="Password=secret"

sed -i "s|$defaultDbPwd1|POSTGRES_PASSWORD:-$DBPASSWORD|g" ./eve-whmapper/docker-compose.yml
sed -i "s|$defaultDbPwd2|Password=$DBPASSWORD|g" ./eve-whmapper/docker-compose.yml

defaultSSOClientId="EveSSO__ClientId=xxxxxxxxx"
sed -i "s|$defaultSSOClientId|EveSSO__ClientId=$SSOCLIENTID|g" ./eve-whmapper/docker-compose.yml

defaultSSOSecret="EveSSO__Secret=xxxxxxxxx"
sed -i "s|$defaultSSOSecret|EveSSO__Secret=$SSOSECRET|g" ./eve-whmapper/docker-compose.yml

echo "Initializing..."

echo "Download Images ..."
docker-compose -f ./haproxy/docker-compose.yml pull
docker-compose -f ./eve-whmapper/docker-compose.yml pull

echo "Prepare container"
docker-compose -f ./eve-whmapper/docker-compose.yml up --no-start
docker-compose -f ./haproxy/docker-compose.yml up --no-start


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
