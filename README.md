# <img src="WHMapper/wwwroot/favicon.ico" width="32" heigth="32"> EvE-WHMapper
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT) ![GitHub top language](https://img.shields.io/github/languages/top/pfh59/eve-whmapper) ![GitHub language count](https://img.shields.io/github/languages/count/pfh59/eve-whmapper) [![Continous Integration and Deployement](https://github.com/pfh59/eve-whmapper/actions/workflows/ci-cd.yaml/badge.svg)](https://github.com/pfh59/eve-whmapper/actions/workflows/ci-cd.yaml)	[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=pfh59_eve-whmapper&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=pfh59_eve-whmapper) ![GitHub commit activity (main)](https://img.shields.io/github/commit-activity/m/pfh59/eve-whmapper)


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
  
### Domain

We recommand to use a domaine name (your.domaine.com) with public DNS.

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

## Get started

EvE-WHMapper can be install using **docker** or **kubernetes**. 
Both method run on multi platform (linux/amd64, linux/arm64)

/!\ make sure [Requirements](#requirements) step is done before continue /!\ 

* [Docker installation](deploy/docker/README.md)
* [Kubernetes installation](deploy/kubernetes/README.md)

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

[![view - Documentation](https://img.shields.io/badge/view-Documentation-blue?style=for-the-badge)](/docs/index.md "Go to project documentation")

</div>

## License

This project is released under [MIT](/LICENSE) by [@pfh59](https://github.com/pfh59)




