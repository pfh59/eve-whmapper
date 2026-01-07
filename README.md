# EvE-WHMapper
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT) ![GitHub top language](https://img.shields.io/github/languages/top/pfh59/eve-whmapper) ![GitHub language count](https://img.shields.io/github/languages/count/pfh59/eve-whmapper) [![Continous Integration and Deployment](https://github.com/pfh59/eve-whmapper/actions/workflows/ci-cd.yaml/badge.svg)](https://github.com/pfh59/eve-whmapper/actions/workflows/ci-cd.yaml) [![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=pfh59_eve-whmapper&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=pfh59_eve-whmapper) ![GitHub commit activity (main)](https://img.shields.io/github/commit-activity/m/pfh59/eve-whmapper) ![GitHub Release](https://img.shields.io/github/v/release/pfh59/eve-whmapper) ![Downloads (all assets, all releases)](https://img.shields.io/github/downloads/pfh59/eve-whmapper/total) ![GitHub Downloads (all assets, latest release)](https://img.shields.io/github/downloads/pfh59/eve-whmapper/latest/total)

EvE-WHMapper is a lightweight wormhole mapping and planning tool for EVE Online. It was originally inspired by [Pathfinder](https://github.com/exodus4d/pathfinder) and has been redesigned to be easier to deploy, maintain, and use in modern fleets and wormhole groups.

Key goals:
- Lightweight and fast
- Simple to deploy and administrate
- Easy to maintain over time

## Core Features

### Authentication
- CCP SSO login
- ESI API integration
- Multi-account support

### Access Control & Administration
- Define administration access lists per character
- Control Eve-WHMapper access for alliances, corporations, and characters
- Configure per-map access lists

### Map Module
- Share maps with other players and alts
- Track pilots and their current locations
- Automatic location tracking via ESI
- Auto-tag multi-system targets with the same class using A/B/C markers (or adjust manually with `+` / `-` or `Up` / `Down` keys)
- Add systems manually via right-click
- Select multiple systems with `Ctrl` + left click and move them together
- Link two systems by selecting them and pressing `L`
- Edit existing system connection links
- Lock systems to prevent accidental moves

### Route Planner Module
- Calculate optimal travel paths between systems—using both stargates and mapped wormholes
- Support route planning for individuals or fleets moving across your chain

### Jump Module
- Track and summarize all jumps between two systems
- Display hole size and current status (e.g., Normal / Critical)
- Show first and last jump information
- Keep a detailed log of each pilot and ship that passes through

## Hosting & Public Instance

EvE-WHMapper can be self-hosted using Docker, Kubernetes, or other deployment options described in the documentation.

For hosted usage, a public instance is available at [https://www.whmapper.ovh](https://www.whmapper.ovh), where you can create and manage your own Eve WHMapper space for an alliance, corporation, or personal use.

## Documentation

Full documentation (installation, configuration, modules, and usage guides) is available at: [https://pfh59.github.io/eve-whmapper-docs/home](https://pfh59.github.io/eve-whmapper-docs/home).

## Contributing

Contributions are welcome. Please see the repository's contributing guidelines in `CONTRIBUTING.md` before opening issues or pull requests.

## License

This project is released under the [MIT License](https://opensource.org/licenses/MIT).