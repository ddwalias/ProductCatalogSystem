# Product Catalog System

Production-minded catalog assessment built as a modular monolith with:

- ASP.NET Core Web API on .NET 10
- Entity Framework Core with SQL Server
- .NET Aspire AppHost for orchestration
- Aspire Docker Compose publishing integration
- Svelte 5 + Vite frontend

## What is included

- Product CRUD with pagination, sorting, category filtering, Elasticsearch-backed search on list queries, and a separate autocomplete endpoint
- Category hierarchy with deterministic ordering and cycle prevention
- Inventory tracking with immutable `InventoryTransactions`
- Product optimistic concurrency via `rowversion`
- Human-readable product revisions via `VersionNumber`
- Temporal history enabled for `Products`
- Elasticsearch-backed search for `GET /api/products?query=...`
- Product-level flexible attributes stored as JSON
- OpenAPI generation in development
- Built-in Swagger-based API explorer in development at `/swagger`
- Health checks and structured write logging
- Seed data for local verification
- Focused automated tests for paging and business-rule validation

## Run locally

Prerequisite: Docker must have the `buildx` plugin available because Aspire uses Dockerfile-based image builds for the local SQL Server full-text image.

On Arch Linux:

```bash
sudo pacman -S docker-buildx
```

You can verify it with:

```bash
docker buildx version
```

From the repository root:

```bash
aspire run
```

Aspire starts:

- SQL Server
- Elasticsearch
- `ProductCatalogSystem.Server`
- the Vite frontend
- the Aspire dashboard

The frontend and API endpoints are injected by Aspire at runtime. During local development, the frontend uses the Vite dev server and proxies `/api` requests to the server.

## Docker Compose publishing

The app host now defines a Docker Compose environment named `compose`. Aspire can publish the existing SQL Server, Elasticsearch, server, and gateway resources into a generated Docker Compose deployment.

During local development, the frontend still runs through the Vite dev server. During Aspire publish/deploy flows, the frontend build output is bundled into the server container so the generated Docker Compose deployment does not depend on a separate Vite service.

Generate the compose artifacts without starting containers:

```bash
aspire publish
```

Build the images and prepare the compose environment:

```bash
aspire do prepare-compose
```

Build the images and start the generated Docker Compose deployment:

```bash
aspire deploy
```

Stop the generated Docker Compose deployment:

```bash
aspire do docker-compose-down-compose
```

## Test and build

Backend build and tests:

```bash
dotnet build ProductCatalogSystem.sln
dotnet test ProductCatalogSystem.sln --no-build
```

Frontend build:

```bash
cd frontend
npm install
npm run build
```

## Key API routes

- `GET /api/products`
- `GET /api/products?query=wireless`
- `GET /api/products/{id}`
- `POST /api/products`
- `PUT /api/products/{id}`
- `DELETE /api/products/{id}`
- `GET /api/products/search`
- `GET /api/categories`
- `POST /api/categories`
- `PUT /api/categories/{id}`
- `GET /api/categories/{id}/products`
- `GET /health`
- `GET /swagger`

## Notes

- The database schema is created through EF Core migrations in `ProductCatalogSystem.Server/Data/Migrations`.
- The API applies migrations and seeds sample data on startup.
- In development, the interactive API explorer is available at `/swagger`.
- Local HTTPS developer certificates are not fully trusted in this environment. HTTP endpoints still work; if you want trusted local HTTPS on Linux, configure `SSL_CERT_DIR` as suggested by the `aspire run` output.
