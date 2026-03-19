# Deployment Notes

This document records the exact deployment approach used for the current live environment.

Current public URL:

- `https://product-catalog-system.duydanghoang.dev`

Current public IP:

- `161.118.227.227`

Current target host:

- OCI Ubuntu ARM64 VM

## Summary

The final deployment is a hybrid setup:

- SQL Server runs inside an emulated `x86_64` Ubuntu guest on the ARM VM
- `ProductCatalogSystem.Server` runs as a native `linux/arm64` container
- `ProductCatalogSystem.Gateway` runs as a native `linux/arm64` container
- the frontend is served through YARP
- HTTPS is terminated directly in the gateway with Kestrel and a Let's Encrypt certificate

This works, but the SQL Server part is still an unsupported workaround because Microsoft SQL Server does not support ARM64 Linux hosts directly.

## Why This Was Necessary

The original app host publish output generates Docker Compose services that target SQL Server in a container. On this ARM VM:

- direct `mcr.microsoft.com/mssql/server:2022-latest` on the host failed
- changing only the OS image was not an option because the underlying CPU is still ARM64

To keep SQL Server on a single ARM machine, a full `x86_64` guest VM was created under QEMU and SQL Server was run inside that guest.

## App Host And Compose

The app uses Aspire for orchestration. The generated compose file comes from:

- [docker-compose.yaml](/home/walias/Projects/ProductCatalogSystem/ProductCatalogSystem.AppHost/aspire-output/docker-compose.yaml)

Relevant app host source:

- [AppHost.cs](/home/walias/Projects/ProductCatalogSystem/ProductCatalogSystem.AppHost/AppHost.cs)

The publish flow used here was:

```bash
aspire publish
```

The generated compose artifacts were copied to the server under:

```bash
/home/ubuntu/productcatalog
```

## Remote Host Layout

On the ARM VM:

- compose stack directory: `/home/ubuntu/productcatalog`
- compose env file: `/home/ubuntu/productcatalog/.env`
- compose override file: `/home/ubuntu/productcatalog/docker-compose.override.yaml`
- TLS cert directory for the gateway: `/home/ubuntu/productcatalog/certs`

## SQL Server On ARM Through QEMU

Because SQL Server could not run directly on the ARM host, a QEMU guest was created:

- guest root: `/home/ubuntu/x86-sqlvm`
- systemd unit: `sqlvm.service`
- guest SSH forward: `127.0.0.1:2222 -> guest:22`
- guest SQL forward: `0.0.0.0:11433 -> guest:1433`

Inside the guest:

- Docker is installed
- SQL Server runs as container `sqlvm-mssql`
- the SQL data files live under `/var/opt/mssql-container`

The application containers do not connect to `sql:1433`. They connect to the ARM host bridge gateway on port `11433`, which QEMU forwards into the guest SQL instance.

## Native ARM App Images

The generated compose output initially pinned the app tier to `linux/amd64`, which caused instability under emulation.

To fix that, `server` and `gateway` were rebuilt locally as native ARM images and loaded onto the VM.

Commands used locally:

```bash
dotnet restore ProductCatalogSystem.Server/ProductCatalogSystem.Server.csproj -r linux-arm64
dotnet publish ProductCatalogSystem.Server/ProductCatalogSystem.Server.csproj -c Release --os linux --arch arm64 /t:PublishContainer -p:ContainerRepository=server -p:ContainerImageTag=latest

dotnet restore ProductCatalogSystem.Gateway/ProductCatalogSystem.Gateway.csproj -r linux-arm64
dotnet publish ProductCatalogSystem.Gateway/ProductCatalogSystem.Gateway.csproj -c Release --os linux --arch arm64 /t:PublishContainer -p:ContainerRepository=gateway -p:ContainerImageTag=latest
```

Images were copied to the host with:

```bash
docker save server:latest | ssh ubuntu@161.118.227.227 'sudo docker load'
docker save gateway:latest | ssh ubuntu@161.118.227.227 'sudo docker load'
```

## Compose Override Used On The Server

The live deployment depends on a compose override on the VM. The important changes are:

- force `server` and `gateway` to `linux/arm64`
- make `server` connect to SQL at `172.18.0.1:11433`
- enable production seed data with `SeedCatalogOnStartup=true`
- expose gateway on `80` and `443`
- mount the HTTPS certificate into the gateway

Current live override path:

- `/home/ubuntu/productcatalog/docker-compose.override.yaml`

Conceptually, the override does this:

```yaml
services:
  server:
    platform: linux/arm64
    environment:
      ConnectionStrings__catalogdb: "Server=172.18.0.1,11433;..."
      SeedCatalogOnStartup: "true"

  gateway:
    platform: linux/arm64
    ports:
      - "80:8080"
      - "443:8443"
    environment:
      ASPNETCORE_URLS: "http://+:8080;https://+:8443"
      ASPNETCORE_Kestrel__Certificates__Default__Path: "/https/product-catalog-system.duydanghoang.dev.pfx"
    volumes:
      - "/home/ubuntu/productcatalog/certs:/https:ro"
```

## Server Startup And Seeding

The server was updated so production seeding can be enabled explicitly with:

- `SeedCatalogOnStartup=true`

Relevant code:

- [Program.cs](/home/walias/Projects/ProductCatalogSystem/ProductCatalogSystem.Server/Program.cs)
- [DbInitializer.cs](/home/walias/Projects/ProductCatalogSystem/ProductCatalogSystem.Server/Data/DbInitializer.cs)

The current seed set was also refined to:

- create `10` categories
- create `100` unique products
- avoid duplicated seed records

## Gateway HTTPS

HTTPS is handled directly by the gateway. Nginx is not used.

Relevant gateway code:

- [Program.cs](/home/walias/Projects/ProductCatalogSystem/ProductCatalogSystem.Gateway/Program.cs)

The gateway was updated to:

- redirect HTTP to HTTPS
- enable HSTS outside development
- listen on both `8080` and `8443` inside the container

## Let's Encrypt Certificate

`certbot` was installed on the ARM VM and used in standalone mode to issue the certificate for:

- `product-catalog-system.duydanghoang.dev`

Certificate files managed by Let's Encrypt:

- `/etc/letsencrypt/live/product-catalog-system.duydanghoang.dev/fullchain.pem`
- `/etc/letsencrypt/live/product-catalog-system.duydanghoang.dev/privkey.pem`

The gateway consumes a PFX file generated from those PEM files:

- `/home/ubuntu/productcatalog/certs/product-catalog-system.duydanghoang.dev.pfx`

There is also a renewal hook that regenerates the PFX and recreates the gateway after renewal:

- `/etc/letsencrypt/renewal-hooks/deploy/productcatalog-gateway.sh`

## DNS And OCI Networking

DNS record used:

- `A product-catalog-system.duydanghoang.dev -> 161.118.227.227`

OCI ingress rules required:

- inbound `TCP 80` from `0.0.0.0/0`
- inbound `TCP 443` from `0.0.0.0/0`

Host firewall rules were also adjusted on the VM to allow:

- `80`
- `443`
- internal app-to-host SQL traffic on `11433` from the Docker subnet

The rules were persisted with:

```bash
sudo netfilter-persistent save
```

## Deployment Commands Used On The Server

Typical restart command:

```bash
cd /home/ubuntu/productcatalog
sudo docker compose -f docker-compose.yaml -f docker-compose.override.yaml up -d --no-deps --force-recreate server gateway
```

Check status:

```bash
cd /home/ubuntu/productcatalog
sudo docker compose -f docker-compose.yaml -f docker-compose.override.yaml ps
```

Check logs:

```bash
sudo docker logs --tail 200 productcatalog-server-1
sudo docker logs --tail 200 productcatalog-gateway-1
```

## Verification Performed

The following were verified after deployment:

- `http://127.0.0.1/` returned `200`
- `http://127.0.0.1/api/products` returned `200`
- `http://product-catalog-system.duydanghoang.dev/` redirects to HTTPS
- `https://product-catalog-system.duydanghoang.dev/` returns `200`
- the TLS certificate is valid for `product-catalog-system.duydanghoang.dev`
- the API reports `100` products
- the category tree contains `10` categories

## Important Caveats

- SQL Server on this environment is still running through full-system `x86_64` emulation on an ARM host.
- This is not a standard supported SQL Server production topology.
- `SeedCatalogOnStartup` is currently enabled in the live compose override to ensure the seeded dataset exists. Once the dataset is stable, it can be turned off to reduce startup work.

## Suggested Next Cleanup

- rebuild `webfrontend` as native ARM too, so the entire app tier is native on the ARM host
- turn `SeedCatalogOnStartup` back off after the desired seed state is stable
- move secret values out of ad hoc files and into a proper secret-management path
