# Product Catalog System

Product catalog system assessment built as a modular monolith with:

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

From the repository root:

```bash
aspire run
```

A local Aspire run exposes the gateway over HTTP. Use the URL shown by Aspire, for example:

```text
http://gateway-productcatalogsystem.dev.localhost:5520
```

Aspire starts:

- SQL Server
- Elasticsearch
- `ProductCatalogSystem.Server`
- the Vite frontend dev server
- the YARP gateway
- the Aspire dashboard

The frontend and API endpoints are injected by Aspire at runtime. During local development, the frontend uses the Vite dev server and proxies `/api` requests to the server.

## Docker Compose publishing

The app host now defines a Docker Compose environment named `compose`. Aspire can publish the existing SQL Server, Elasticsearch, server, and gateway resources into a generated Docker Compose deployment.

During local development, the frontend runs through the Vite dev server. In publish mode, the app host switches to a Dockerfile-based frontend resource so the generated compose deployment includes:

- `sql`
- `elasticsearch`
- `server`
- `webfrontend`
- `gateway`

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
dotnet test ProductCatalogSystem.sln
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
- `GET /api/products/search`
- `GET /api/products/{id}`
- `POST /api/products`
- `PUT /api/products/{id}`
- `DELETE /api/products/{id}`
- `GET /api/categories`
- `GET /api/categories/{id}/products`
- `POST /api/categories`
- `PUT /api/categories/{id}`
- `GET /api/health`

## Database and seed data

- The database schema is created through EF Core migrations in `ProductCatalogSystem.Server/Data/Migrations`.
- The API applies migrations on startup.
- In development, the API seeds sample data automatically.
- In production, seed data runs only when `SeedCatalogOnStartup=true`.
- The current seed set creates `10` categories and `100` unique products.

## Notes

- In development, the interactive API explorer is available at `/swagger`.
- Local HTTPS developer certificates are not fully trusted in this environment. HTTP endpoints still work; if you want trusted local HTTPS on Linux, configure `SSL_CERT_DIR` as suggested by the `aspire run` output.
- Search indexing uses Elasticsearch when available. If Elasticsearch is unhealthy, the API falls back to database-backed product queries instead of failing startup.
