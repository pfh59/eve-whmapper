# Eve WHMapper Local Deployment

This guide explains how to deploy Eve WHMapper locally using Docker and Nginx.

## Prerequisites

- **Docker** and **Docker Compose** installed
- **mkcert** installed for generating local SSL certificates
- **PowerShell** (Windows) or compatible shell
- **You must add `whmapper.local` to your `C:\Windows\System32\drivers\etc\hosts` file with your private IP address**  
  Example:
  ```
  192.168.x.x whmapper.local
  ```
  Replace `192.168.x.x` with your actual private IP.

## Deployment Steps

### 1. Clone the Repository

```sh
git clone https://github.com/your-org/eve-whmapper.git
cd eve-whmapper/deploy/localhost/docker
```

### 2. Initialize the Environment

This script will:
- Remove previous containers and volumes
- Generate a self-signed SSL certificate for `whmapper.local`
- Prompt for database and EVE SSO credentials
- Prepare configuration files

**Windows:**
```powershell
.\init.ps1
```

**macOS/Linux:**
```sh
pwsh ./init.ps1
```

### 3. Start the Containers

```powershell
.\start.ps1
```
or
```sh
pwsh ./start.ps1
```

### 4. Access the Application

Open your browser and go to:

```
https://whmapper.local
```

> You may need to trust the generated certificate in your browser.

### 5. Stop the Containers

```powershell
.\stop.ps1
```
or
```sh
pwsh ./stop.ps1
```

## Troubleshooting

- Ensure required files exist:
  - `nginx/conf/nginx.conf`
  - `nginx/certs/whmapper.local.pem`
  - `nginx/certs/whmapper.local.key`
  - `docker-compose.yml`
- If you see SSL errors, make sure your browser trusts the local CA installed by `mkcert`.
- Make sure `whmapper.local` is mapped to your private IP in your hosts file.

---