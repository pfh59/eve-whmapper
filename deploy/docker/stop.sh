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


echo "Stoping..."
echo "Stoping backend..."
docker-compose -f ./eve-whmapper/docker-compose.yml stop

echo "Stoping frontend..."
docker-compose -f ./haproxy/docker-compose.yml stop
