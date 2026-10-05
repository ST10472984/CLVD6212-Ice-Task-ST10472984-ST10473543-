# Inventory & Reorder Tracker

Full-stack cloud application built for the Cloud Development group assignment.

## The Problem

Small retailers, cafés and small warehouses frequently track stock using paper logs or
ad-hoc spreadsheets rather than dedicated software. This creates two recurring, costly
failure modes. First, stockouts: a popular item quietly runs out and nobody notices until
a customer asks for it, by which point a sale (and often the customer's future business)
is lost. Second, overstocking: without visibility into what is actually running low,
staff over-order "just in case," tying up cash in inventory that sits on a shelf — a
particularly expensive mistake for perishable goods.

These businesses are underserved by existing software. Full ERP and inventory-management
platforms are built for larger operations, come with subscription costs and a learning
curve that a five-person café or corner shop cannot justify. What these businesses need
instead is a lightweight, purpose-built tool: a way to see current stock at a glance, log
what comes in and goes out, and get notified automatically when something needs
reordering — without hiring anyone or paying for enterprise software.

**Target users:** small business owners and the staff who manage day-to-day stock —
people who are not necessarily technical, and need something simple enough to use between
serving customers.

**How this application solves it:** the Inventory & Reorder Tracker centralizes stock
levels in one place, lets staff log every stock movement (deliveries received, items
sold, stock adjustments or wastage) against a product, and automatically flags any
product that has fallen to or below its reorder threshold. When that happens, the system
sends an email alert, so staff are notified without having to actively check the
dashboard. A live dashboard view also surfaces every low-stock item at a glance.

## Live Application

**URL:** _add the Render URL here once both services are deployed and confirmed working_

## Architecture

- **Backend:** Azure Functions (.NET 8, isolated worker model). HTTP-triggered endpoints
  handle Products CRUD and stock movements; a scheduled Timer trigger (plus a manual
  on-demand endpoint, for environments where the scheduler isn't reliable) checks for
  low stock and triggers an email alert.
- **Frontend:** ASP.NET Core MVC, calling the Functions API through a typed `HttpClient`.
  Provides a dashboard of currently low-stock items and full product management.
- **Database:** PostgreSQL, accessed via Entity Framework Core and Npgsql. Locally, a
  containerized Postgres instance runs through Docker Compose; in production, a hosted
  free-tier provider (Supabase/Neon) is used instead, since a database running inside a
  container does not survive a redeploy on most free hosting platforms.
- **Local storage emulator:** Azurite, required by the Functions host locally for the
  Timer trigger's internal state. Not needed once deployed, since real Azure/hosting
  environments provide this differently (or the manual trigger endpoint is used instead).
- **Third-party service:** SendGrid, for sending the low-stock email alert.
- **Containerization:** each service (Functions API, MVC frontend) has its own
  `Dockerfile`; a root `docker-compose.yml` orchestrates both plus Postgres and Azurite
  for local development.
- **CI/CD:** GitHub Actions builds and tests both projects, runs the test suite, validates
  the Docker build, scans for vulnerabilities with Trivy, and — on a successful push to
  `master` — triggers a deploy on Render for both services.
[ MVC frontend ] --HTTP--> [ Azure Functions API ] --EF Core / Npgsql--> [ PostgreSQL ]
|
+--> [ SendGrid ] (low-stock email alerts)

## Running Locally

1. Copy `.env.example` to `.env` in the repo root and fill in real values. A placeholder
   SendGrid key is fine for local use — the alert service logs a warning and skips
   sending rather than failing.
2. From the repo root:
docker compose up --build
3. Once running:
   - MVC frontend: http://localhost:8080
   - Functions API: http://localhost:7071/api/products
   - API documentation (Swagger UI): http://localhost:7071/api/swagger/ui

## Environment Variables

See `.env.example` for the full list with placeholder values. Required:

| Variable | Purpose |
|---|---|
| `POSTGRES_DB` / `POSTGRES_USER` / `POSTGRES_PASSWORD` | Local Postgres container credentials |
| `SENDGRID_API_KEY` | SendGrid API key for sending alert emails |
| `ALERT_EMAIL_FROM` | Verified sender address in SendGrid |
| `ALERT_EMAIL_TO` | Address that receives low-stock alerts |

The deployed version on Render additionally requires `ConnectionStrings__Default` (the
hosted Postgres connection string) and `InventoryApi__BaseUrl` (the deployed Functions
API's URL) — set directly as environment variables in each Render service, not in `.env`.

## Project Layout

- `backend-functions/` — Azure Functions app: Products and StockMovements HTTP APIs, the
  low-stock Timer trigger and its manual on-demand equivalent, EF Core `InventoryDbContext`
- `backend-functions.Tests/` — unit tests for stock validation logic and the low-stock query
- `frontend-mvc/` — ASP.NET Core MVC app: dashboard, Products CRUD, stock movement logging
- `docker-compose.yml` — local orchestration: Postgres, Azurite, Functions, MVC
- `.github/workflows/ci-cd.yml` — build, test, Docker build validation, security scan, deploy

## Group Members and Contributions
Rohith Maharaj ST10473543
Yuveer Arjoon ST10472984
