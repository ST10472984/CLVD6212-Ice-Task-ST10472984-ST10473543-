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

**URL:** _add the Render URL here once the application is deployed and confirmed working_

## Architecture

- **Backend:** Azure Functions (.NET 8, isolated worker model). HTTP-triggered endpoints
  handle Products CRUD and stock movements, while the low-stock functionality checks
  inventory levels and triggers email alerts.
- **Database:** PostgreSQL, accessed through Entity Framework Core and Npgsql.
- **Data layer:** The `Data/` folder contains the database context and data-access
  components used to communicate with PostgreSQL.
- **Functions:** The `Functions/` folder contains the Azure Functions and HTTP-triggered
  endpoints responsible for handling application requests.
- **Models:** The `Models/` folder contains the application's data models and entities.
- **Services:** The `Services/` folder contains business logic and supporting services,
  including functionality related to low-stock notifications.
- **Third-party service:** SendGrid is used for sending low-stock email alerts.
- **Containerization:** The root `Dockerfile` is used to build the application as a
  container image.
- **CI/CD:** GitHub Actions is configured through `.github/workflows/` to build the
  application, validate the Docker image, run security scanning with Trivy, and trigger
  deployment when the required deployment configuration is available.

[ Functions / API ]
|
+--> [ Data / EF Core / Npgsql ] --> [ PostgreSQL ]
|
+--> [ Services ] --> [ SendGrid ]
|
+--> [ Models ]


## Running Locally

1. Make sure the .NET 8 SDK is installed on your machine.
2. From the repository root, restore the project dependencies:

dotnet restore InventoryTracker.Functions.csproj

3. Build the application:

dotnet build InventoryTracker.Functions.csproj

4. Configure your local application settings using `local.settings.json`.
   The `local.settings.json.example` file can be used as a template.
5. Run the Azure Functions application using your preferred .NET/Azure Functions
   development setup.

## Environment Variables

See `local.settings.json.example` for the available configuration values and placeholder
settings.

The application may require configuration for the following services:

| Variable | Purpose |
|---|---|
| `ConnectionStrings__Default` | PostgreSQL database connection string |
| `SENDGRID_API_KEY` | SendGrid API key used for sending alert emails |
| `ALERT_EMAIL_FROM` | Sender address used for low-stock alerts |
| `ALERT_EMAIL_TO` | Address that receives low-stock alerts |

Deployment-specific values, such as the Render deployment configuration, should be
configured through the hosting platform or GitHub repository secrets rather than being
stored directly in the source code.

## Project Layout

CLVD6212-Ice-Task-ST10472984-ST10473543-
│
├── .github/
│ └── workflows/
│ └── ci-cd.yml
│
├── Data/
│ └── Database context and data-access components
│
├── Functions/
│ └── Azure Functions and HTTP-triggered endpoints
│
├── Models/
│ └── Application models and entities
│
├── Services/
│ └── Business logic and supporting services
│
├── .dockerignore
├── .gitattributes
├── .gitignore
├── Dockerfile
├── InventoryTracker.Functions.csproj
├── InventoryTracker.Functions.slnx
├── Program.cs
├── README.md
├── host.json
├── local.settings.json
└── local.settings.json.example


- `.github/workflows/` — GitHub Actions workflow configuration for building,
  validating, scanning and deploying the application.
- `Data/` — database context and data-access functionality.
- `Functions/` — Azure Functions and HTTP-triggered API endpoints.
- `Models/` — application models and entities.
- `Services/` — business logic and supporting services.
- `Dockerfile` — configuration used to build the application as a Docker container.
- `InventoryTracker.Functions.csproj` — .NET project configuration and dependencies.
- `InventoryTracker.Functions.slnx` — solution file for the project.
- `Program.cs` — application startup and dependency-injection configuration.
- `host.json` — Azure Functions host configuration.
- `local.settings.json` — local Azure Functions configuration.
- `local.settings.json.example` — example configuration file for local development.
- `README.md` — project documentation.

## Group Members and Contributions

- Rohith Maharaj — ST10473543
- Yuveer Arjoon — ST10472984
