# <img src="WHMapper/wwwroot/favicon.ico" width="32" heigth="32"> EvE-WHMapper
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT) ![GitHub top language](https://img.shields.io/github/languages/top/pfh59/eve-whmapper) ![GitHub language count](https://img.shields.io/github/languages/count/pfh59/eve-whmapper) [![Continous Integration and Deployement](https://github.com/pfh59/eve-whmapper/actions/workflows/ci-cd.yaml/badge.svg)](https://github.com/pfh59/eve-whmapper/actions/workflows/ci-cd.yaml)	[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=pfh59_eve-whmapper&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=pfh59_eve-whmapper) ![GitHub commit activity (main)](https://img.shields.io/github/commit-activity/m/pfh59/eve-whmapper)


# Deploy on kubernate

This section is dedicated for deploy the mapper on kubernate.
If u have already k3s set u can go directly to the **Deploy** section

This was made and tested on RasphberryPi cluster.

## Requirements

### Kubernates

#### Install k3s

```
apt update
curl -sfL https://get.k3s.io | sh -
```

#### Get node-token

cmd: `cat /var/lib/rancher/k3s/server/node-token` 

#### Ingress

cmd: `kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.9.5/deploy/static/provider/cloud/deploy.yaml`

#### Cert Manager

cmd: `kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.3/cert-manager.yaml`

Anny issue with certificates?
Use:
List all certs: `sudo kubectl get ClusterIssuers,Certificates,CertificateRequests,Orders,Challenges --all-namespaces`
Detail of evemapper cert status: `sudo kubectl describe certificaterequest tls-evemapper-1 -n evemapper`

#### Portainer (optional)

```
kubectl apply -n portainer -f https://raw.githubusercontent.com/portainer/k8s/master/deploy/manifests/portainer/portainer.yaml
kubectl apply -f https://downloads.portainer.io/ce2-19/portainer-agent-k8s-lb.yaml
```

### Ports (homemade server)

If u have deployed it at home, u must forward ports of the application to the K3s cluster:
- 80:443 TCP/UDP <your k3s server (master)>
- 443:443 TCP/UDP <your k3s server (master)>

## Deploy

The deploy is split into many files to apply after configuration.

### Edit envs

All service use a same ConfigMap with in **deploys/envs.yaml [ConfigMap]**:
- DOMAIN:  <your domain (ex: map.mycorp.com)>
- POSTGRES_DB: <your db name>
- POSTGRESQL_USERNAME: <your db user>
- POSTGRESQL_PASSWORD: <your db password>
- POSTGRESQL_DATABASE: <same as POSTGRES_DB>
- EveSSO__ClientId: <your client ID given from CCP>
- EveSSO__Secret: <your secret key given from CCP>
- ConnectionStrings__DatabaseConnection: server=<cluster-local-ip>;port=31252;database=<same as POSTGRES_DB>;User Id=<same as POSTGRESQL_USERNAME>;Password=<same as POSTGRESQL_PASSWORD>
- ConnectionStrings__RedisConnection: <cluster-local-ip>:31253

Remarks:
- `cluster-local-ip` is the local ip of the master node is your network (not internet machine IP). For me its 192.168.1.50 (master node) 
- For sensitive values there is a Secret for storing them but values must be encoded in base_64

### Edit Certificate

Env can not be used directly in the deployement file. 
So u need to modify some value deeper in the file.

In **deploys/base.yaml [Certificate-issuer]**:
- email: need to put a valid email (using to get notified when certificat is going to be outdated)

In **deploys/base.yaml [Certificate]**:
- commonName: change it to your subdomain (same as `DOMAIN` for envs)
```
    ex:
        commonName: map.mycorp.com
```

- dnsNames: list of all dns that u want to use. **commonName** must appear in the list
  
```
  ex:
    - map.mycorp.com
    - mappper.mycorp.com
```

### Edit Ingress

In **deploys/ingress.yaml [evemapper-ingress]**: replace both [your SUB/DOMAIN] by your sub/domain


### Apply

All deployements are located in deploys directory but we can deploy all at same time using the following command: `kubectl apply -f deploys`

U may need to launch the deploy 2 times because at some service not waiting for namespace to be done before trying to create themself.

Result of first run:
```
namespace/evemapper created
issuer.cert-manager.io/letsencrypt-evemapper created
certificate.cert-manager.io/tls-evemapper created
configmap/evemapper-config-map created
secret/evemapper-secrets created
deployment.apps/evemapper-app created
service/evemapper-app-service created
ingress.networking.k8s.io/evemapper-ingress created
Error from server (NotFound): error when creating "deploys/backend.yaml": namespaces "evemapper" not found
Error from server (NotFound): error when creating "deploys/backend.yaml": namespaces "evemapper" not found
Error from server (NotFound): error when creating "deploys/backend.yaml": namespaces "evemapper" not found
Error from server (NotFound): error when creating "deploys/backend.yaml": namespaces "evemapper" not found
```

Second result run:
```
deployment.apps/postgres created
service/postgres-cluster-ip-service created
deployment.apps/redis created
service/redis-cluster-ip-service created
namespace/evemapper unchanged
issuer.cert-manager.io/letsencrypt-evemapper unchanged
certificate.cert-manager.io/tls-evemapper unchanged
configmap/evemapper-config-map unchanged
secret/evemapper-secrets unchanged
deployment.apps/evemapper-app unchanged
service/evemapper-app-service unchanged
ingress.networking.k8s.io/evemapper-ingress unchanged
```

### Update app & restart

U can use the following cmd: `kubectl rollout restart deployment evemapper-app -n evemapper`
It will redeploy the frontend only

U can also use portainer to redeploy the frontend (evemapper-app)

## Improve  feature?

- FIX order of yaml files execution using .metadata.labels.order
- use volume for db
  
  