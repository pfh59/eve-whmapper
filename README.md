# eve-whmapper [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT) ![GitHub top language](https://img.shields.io/github/languages/top/pfh59/eve-whmapper) ![GitHub language count](https://img.shields.io/github/languages/count/pfh59/eve-whmapper) [![Continous Integration and Deployement](https://github.com/pfh59/eve-whmapper/actions/workflows/ci-cd.yaml/badge.svg)](https://github.com/pfh59/eve-whmapper/actions/workflows/ci-cd.yaml)	[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=pfh59_eve-whmapper&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=pfh59_eve-whmapper) ![GitHub commit activity (main)](https://img.shields.io/github/commit-activity/m/pfh59/eve-whmapper) ![GitLab tag (self-managed)](https://img.shields.io/gitlab/v/tag/eve-whmapper) 


## Description

Eve wormhole mapper is a simple mapping tool for eve player. It was inspired by the popular [Pathfinder](https://github.com/exodus4d/pathfinder), but dit not evolved afterwards. Eve-whmapper is more : 
- lightweight
- simple to deploy
- simple to administrate
- faster
- easier to maintain

## Table of contents
* [Technologies](#technologies)
* [Requirements](#requirements)
* [Installation](#installation)
* [Features](#features)
* [Documentation](#documentation)
* [License](#license)

## Technologies

Eve wormhole mapper is written in C# using ASP.NET Core Blazor Server,EF Core, SignalR... and use some third-party components specialy [Blazor.DIAGRAMS](https://blazor-diagrams.zhaytam.com)

## Requirements

### Server

We recommand to linux server with OS [Debian] (https://www.debian.org/index.fr.html)
If you want to use an other OS, you need to write your own init script or configure manually files (see the list bellow)

### Domain

We recommand to use a domaine name (your.domaine.com) with public DNS.

### Docker

In order to run this container you'll need docker installed.

* [Linux](https://docs.docker.com/linux/started/)
* [Windows](https://docs.docker.com/windows/started)
* [OS X](https://docs.docker.com/mac/started/)

### Register your app with CCP

Eve-whmapper requires CCP's SSO authentication API to use [ESI API](https://esi.evetech.net/ui/).

Register your app in https://developers.eveonline.com
- Click to "MANAGE APPLICATIONS" button
- Click to "CREATE NEW APPLICATION" button
- Choose a Name of your choice for your installation (prod eve-whmapper)
- Enter a description for this installation (Eve wormholemapper on production)
- Change "CONNECTION TYPE" to "Authentication & API Access"
- Add Minimun required "PERMISSIONS" (scopes)
  - esi-location.read_location.v1
  - esi-location.read_ship_type.v1
  - esi-ui.open_window.v
  - esi-ui.write_waypoint.v1
  - esi-search.search_structures.v1
- Set your "CALLBACK URL" (https://your.domaine.com/sso/callback)
- Click to "CREATE APPLICATION"
- Copy Client ID,Secret Key and Callback URL to use if on next configuration step

## Installation

### Getting Started

Make sure your server environment fulfils all [Requirements](requirements) first.

#### Get docker-compose template

Navigate to your desired install location and git clone the eve-whmapper repository :
(Recommanded install location : /opt/)

```shell
sudo git clone https://github.com/pfh59/eve-whmapper.git
cd eve-whmapper/docker
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

## Features

### Authentication module
- CCP's SSO login
- Cookie bases login system
- ESI API support
- Multi account support

### Administration Module

- Set administation access list to characters
- Set eve-whmapper access list to alliances/coorporations/characters

### Map Module

- Share map whith other players
- Track Pilots
- Auto Location tracking
- Auto Tag multi system target with same class from same system  with A,B,C or change manualy with '+'/'-' or 'Up'/'Down' Keys
- Add manualy systems via right click
- Select multiple systems by ctrl + left click
- Select and move multiple systems at once
- Link two systems after selecting them and press 'L' Key
- Edit system connection link
- Lock system
- Delete system espect locked system
- Live syncrhonisation between clients

### System Module

- Easy access to system informations
  - Name
  - Security
  - Wormhole Class
  - Wormhole Type
  - Wormhole static information
  - Wormhole effects
    
### Signature Module

- Share system signature informations
- Add/update/delete multiple signature at once 
- Check for new signatures within a second
- Updated signature type information
- Live synchronisation between clients

## Documentation

<div align="center">

[![view - Documentation](https://img.shields.io/badge/view-Documentation-blue?style=for-the-badge)](/docs/ "Go to project documentation")

</div>

## License

This project is released under [MIT](/LICENSE) by [@pfh59](https://github.com/pfh59)




