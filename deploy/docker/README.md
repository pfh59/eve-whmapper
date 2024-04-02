# <img src="WHMapper/wwwroot/favicon.ico" width="32" heigth="32"> EvE-WHMapper
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT) ![GitHub top language](https://img.shields.io/github/languages/top/pfh59/eve-whmapper) ![GitHub language count](https://img.shields.io/github/languages/count/pfh59/eve-whmapper) [![Continous Integration and Deployement](https://github.com/pfh59/eve-whmapper/actions/workflows/ci-cd.yaml/badge.svg)](https://github.com/pfh59/eve-whmapper/actions/workflows/ci-cd.yaml)	[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=pfh59_eve-whmapper&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=pfh59_eve-whmapper) ![GitHub commit activity (main)](https://img.shields.io/github/commit-activity/m/pfh59/eve-whmapper)


# Deploy with docker & compose

## Server

We recommand to linux server with OS [Debian] (https://www.debian.org/index.fr.html)
If you want to use an other OS, you need to write your own init script or configure manually files (see the list bellow)

## Docker

In order to run this container you'll need docker installed.

* [Linux](https://docs.docker.com/linux/started/)
* [Windows](https://docs.docker.com/windows/started)
* [OS X](https://docs.docker.com/mac/started/)

### Get docker-compose template

Navigate to your desired install location and git clone the eve-whmapper repository :
(Recommanded install location : /opt/)

```shell
sudo git clone https://github.com/pfh59/eve-whmapper.git
cd eve-whmapper/deploy/docker
```

### Configuration

Eve-whmapper is pretty easy to configure!

Run the init.sh script as sudo or root user and follow instructions

```shell
sudo ./init.sh
```

This script automatically :
- Updates all the configurations (docker-compose.yml, haproxy.cfg, nginx.conf) from your response,
- Initialize all docker container.
- Create,add and use a strong certifcate to use HTTPS with your domain
  
## Start and stop

- To Start all container, use the script start.sh as a sudo user or root user

```shell
sudo ./start.sh
```

- To Stop all container, use the script start.sh as a sudo user or root user

```shell
sudo ./stop.sh
```