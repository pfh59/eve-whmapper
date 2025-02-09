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

#check prerequisites
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

echo "Starting..."
echo "Starting backend..."
docker-compose -f ./eve-whmapper/docker-compose.yml start

echo "Starting frontend..."
docker-compose -f ./haproxy/docker-compose.yml start
