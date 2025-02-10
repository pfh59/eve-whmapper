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

#clear screen
clear

echo "#------------------------------------------------------------#"
echo "EVE WHMAPPER DEPLOYMENT SCRIPT"
echo "#------------------------------------------------------------#"



echo "Removing existing configuration..."
#remove if exist
if [ -d "./haproxy/certbot" ]; then
    rm -rf ./haproxy/certbot/*
fi

if [ -d "./haproxy/certs" ]; then
    rm -rf ./haproxy/certs/*
fi


echo "#------------------------------------------------------------#"

#check prerequisites
echo "Checking prerequisites..."
if ! command -v docker &> /dev/null
then
    echo "Docker could not be found. Please install Docker and try again."
    exit 1
fi

if ! command -v docker-compose &> /dev/null
then
    echo "Docker-compose could not be found. Please install Docker-compose and try again."
    exit 1
fi

echo "#------------------------------------------------------------#"


echo "Installation Wizard"
echo "Configation Certbot and HAProxy"
read -p "Enter you domain [Required] :" DOMAIN
while [[ -z "$DOMAIN" || ! "$DOMAIN" =~ ^[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$ ]]; do
      echo -e "\e[31mError: Domain must be set and valid (e.g., example.com)\e[0m"
      read -p "Enter your domain [Required] :" DOMAIN
done

read -p "Enter your email (used by certbot to send an email before certificate expiration) [Required] :" EMAIL
while [[ -z "$EMAIL" || ! "$EMAIL" =~ ^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$ ]]; do
      echo -e "\e[31mError: A valid email address must be set (e.g., user@example.com)\e[0m"
      read -p "Enter your email (used by certbot to send an email before certificate expiration) [Required] :" EMAIL
done

read -p 'Enter options (exemple : "-d sub1.your.doamin.com") [OPTIONAL] :' OPTIONS

echo "Configurate Db"
while true; do
      read -s -p "Enter your root db password [Required] :" DBPASSWORD
      echo
      read -s -p "Confirm your root db password [Required] :" CONFIRM_DBPASSWORD
      echo
      if [ -z "$DBPASSWORD" ]; then
            echo -e "\e[31mError: DB PASSWORD must be set\e[0m"
      elif [ "$DBPASSWORD" != "$CONFIRM_DBPASSWORD" ]; then
            echo -e "\e[31mError: Passwords do not match. Please try again.\e[0m"
      else
            break
      fi
done

echo  "\n"
echo "Configuration CCP SSO"

read -p "Enter CCP SSO ClientId [Required] :" SSOCLIENTID
if [ -z "$SSOCLIENTID" ]
then
      echo "Error: SSO CLIENTID must be set"
      exit 1
fi

while true; do
      read -s -p "Enter CCP SSO Secret [Required] :" SSOSECRET
      echo
      read -s -p "Confirm CCP SSO Secret [Required] :" CONFIRM_SSOSECRET
      echo
      if [ -z "$SSOSECRET" ]; then
            echo -e "\e[31mError: SSO SECRET must be set\e[0m"
      elif [ "$SSOSECRET" != "$CONFIRM_SSOSECRET" ]; then
            echo -e "\e[31mError: Secrets do not match. Please try again.\e[0m"
      else
            break
      fi
done

echo "#------------------------------------------------------------#"
#write current configuration

#check if all required files exist
if [ ! -f "./haproxy/nginx/nginx.conf" ]; then
    echo "Error: ./haproxy/nginx/nginx.conf not found"
    exit 1
fi

if [ ! -f "./eve-whmapper/docker-compose.yml" ]; then
    echo "Error: ./eve-whmapper/docker-compose.yml not found"
    exit 1
fi

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

docker-compose -f ./haproxy/docker-compose.yml run --rm  certbot certonly --webroot --webroot-path /var/www/certbot/ -d $DOMAIN --email $EMAIL --non-interactive --agree-tos $OPTIONS

# Merge private key and full chain in one file and add them to haproxy certs folder
function cat-cert() {
      dir="./haproxy/certbot/conf/live/$DOMAIN"
      cat "$dir/privkey.pem" "$dir/fullchain.pem" > "./haproxy/certs/$DOMAIN.pem"
}

echo "Run merge certificate for the requested domain name"
cat-cert "$DOMAIN"

echo "Configura HAProxy HTTPS binding"
find="bind :443"
replace="bind :443 ssl crt /usr/local/etc/certs/$DOMAIN.pem alpn h2"
sed -i "s|$find|$replace|g" ./haproxy/conf/haproxy.cfg

echo "Restart HAPROXY with HTTPS certs"
docker-compose -f haproxy/docker-compose.yml kill -s HUP haproxy


echo "Stoping frontend HAPROXY"
docker-compose -f ./haproxy/docker-compose.yml stop
