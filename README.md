# Enter Engine

A web application for registering vehicles and managing their weight categories.

Users can register a vehicle, view and sort the vehicle list, and manage the category ranges used to classify vehicles.

## Features

- Register vehicles with an owner's name, manufacturer, year of manufacture and weight in kilograms.
- Choose from Toyota, Mazda, Mercedes, Honda and Ferrari.
- View each vehicle's current category and icon.
- Sort vehicles by owner's name, manufacturer, year or weight in ascending or descending order.
- Create, edit and delete categories, including their weight ranges and icons.
- Reject category configurations containing gaps or overlapping ranges.

## Technology

- **Backend:** C#, ASP.NET Core 10 and Entity Framework Core 10.
- **Database:** Microsoft SQL Server.
- **Frontend:** React, TypeScript and Vite.
- **Backend testing:** xUnit, with separate unit and integration test projects.

## Prerequisites

- .NET 10 SDK.
- Node.js 22.12+ or Node.js 24, with npm.
- Microsoft SQL Server 2019 or later, or Docker Desktop to run the included SQL Server 2022 container.
- Git.

Run the following commands from the project root unless stated otherwise.

## Local setup

### 1. Start SQL Server

If you already have SQL Server running locally on port 1433, use that instance and skip this step.

Copy the example environment file from the project root:

```sh
cp .env.example .env
```

Open `.env` and replace `MSSQL_SA_PASSWORD` with a strong local
password before starting Docker Compose.

```sh
docker compose up -d
docker compose ps -a
docker compose logs --tail=100 sqlserver
```

Wait for this message before continuing:

```text
SQL Server is now ready for client connections
```

The Compose configuration requests `linux/amd64` emulation on Apple Silicon Macs. This setup may work locally, but Microsoft does not support SQL Server containers under emulation. If the container cannot start, use SQL Server on a supported Windows or x86-64 Linux host.

Database files are stored in a Docker volume. Running `docker compose down -v` deletes that volume and its data.

### 2. Restore backend dependencies

```sh
dotnet restore backend/EnterEngine.slnx
dotnet tool restore
```

### 3. Configure the connection string

Replace `YOUR_LOCAL_PASSWORD` with your SQL Server password:

```sh
dotnet user-secrets set 'ConnectionStrings:Vehicles' 'Server=localhost,1433;Database=EnterEngineScratch;User Id=sa;Password=YOUR_LOCAL_PASSWORD;Encrypt=True;TrustServerCertificate=True;' --project backend/EnterEngine.Api
```

`EnterEngineScratch` is the local application database name. If you change it, use the same name consistently.

Alternatively, configure the connection using the `ConnectionStrings__Vehicles` environment variable.

Keep credentials out of source control. `TrustServerCertificate=True` is intended for this local development setup.

### 4. Apply database migrations

With SQL Server running:

```sh
dotnet ef database update --project backend/EnterEngine.Api --startup-project backend/EnterEngine.Api
```

This applies the committed migrations and their seed data. A fresh checkout does not require generating a new migration.

### 5. Start the API

```sh
dotnet run --project backend/EnterEngine.Api --no-launch-profile -- --urls http://localhost:5080
```

Leave this terminal running.

The health endpoint is available at:

```text
http://localhost:5080/api/health
```

The health response confirms that the API is running. It does not verify database connectivity.

### 6. Start the frontend

Open a second terminal in the project root:

```sh
cd frontend
npm ci
npm run dev
```

Open the address shown by Vite, normally:

```text
http://localhost:5173
```

During development, Vite forwards `/api` requests to the backend on port 5080.

## Using the application

### Register a vehicle

Enter the owner's name, select a manufacturer, and provide the year of manufacture and weight in kilograms.

Weight must be positive and support no more than two decimal places. The maximum stored weight is `99,999,999.99 kg`.

After saving, the vehicle appears in the vehicle list.

### Sort vehicles

Select a column heading to sort by owner's name, manufacturer, year of manufacture or weight. Select the same heading again to reverse the direction.

An arrow and a description identify the active sort order.

Sorting happens in React after the API returns the vehicle list. Text fields are sorted alphabetically; year and weight are sorted numerically.

### Manage categories

Add, edit or remove category rows, define their weight ranges, and choose their icons. Save the complete configuration after making the required changes.

The API validates the configuration before saving it. An invalid configuration leaves the existing categories unchanged.

## Category rules

Category ranges use an inclusive lower boundary and an exclusive upper boundary: `[minimum, maximum)`.

The initial ranges are:

| Category | Minimum weight, inclusive | Maximum weight, exclusive |
| -------- | ------------------------: | ------------------------: |
| Light    |                      0 kg |                    500 kg |
| Medium   |                    500 kg |                   2500 kg |
| Heavy    |                   2500 kg |            No upper limit |

A vehicle weighing exactly 500 kg is Medium. A vehicle weighing exactly 2500 kg is Heavy.

The configuration must meet these rules:

- At least one category must exist.
- The first range starts at 0 kg, although vehicle weights must be greater than zero.
- Adjacent ranges share the same boundary.
- Ranges cannot overlap or leave gaps.
- Every finite upper boundary must be greater than its lower boundary.
- Only the final range has a null upper boundary, representing no upper limit.
- Boundaries support up to two decimal places.

The complete configuration is saved in one transaction.

Vehicles store their weight rather than a category assignment. The API calculates each vehicle's current category and icon from the saved ranges when retrieving the vehicle list. This allows existing vehicles to reflect changes to category boundaries.

Saving a configuration replaces the category rows. Category IDs may therefore change and are not referenced by vehicles.

## Automated tests

### Unit tests

From the project root:

```sh
dotnet test backend/EnterEngine.UnitTests
```

The unit tests cover:

- Category determination and boundary values.
- Acceptance of valid category configurations.
- Rejection of gaps and overlapping ranges.
- Vehicle request validation, including invalid weights.

### Integration tests

SQL Server must be running. The tests require a separate database connection supplied through `ENTERENGINE_TEST_SQL`.

Replace `YOUR_SQL_PASSWORD` below:

```sh
export ENTERENGINE_TEST_SQL='Server=localhost,1433;Database=EnterEngine_IntegrationTests;User Id=sa;Password=YOUR_SQL_PASSWORD;Encrypt=True;TrustServerCertificate=True;'
```

Then run:

```sh
dotnet test backend/EnterEngine.IntegrationTests
```

The variable remains available for subsequent commands in that terminal session. Set it again when opening a new terminal.

Use a dedicated test database. The test setup creates and deletes test data and may recreate or delete the database. Do not use the application database.

Integration tests cover:

- Creating a vehicle and retrieving it through a separate request.
- Category changes affecting an existing vehicle.
- Invalid weights and unknown manufacturer IDs being rejected without saving.
- Invalid category configurations leaving saved data unchanged.

### Frontend tests

```sh
cd frontend
npm run test:report
```

Frontend tests cover:

- Sorting logic from function exported at `TestResults/Frontend/results.xml`

### Save backend test reports

For unit test results:

```sh
dotnet test backend/EnterEngine.UnitTests --logger trx --results-directory TestResults/Unit
```

For integration test results, first set `ENTERENGINE_TEST_SQL` as shown above, then run:

```sh
dotnet test backend/EnterEngine.IntegrationTests --logger trx --results-directory TestResults/Integration
```

For frontend test results, first ensure terminal is in the project root directory, then run:

```sh
cd frontend
npm run test:report
```

Backend test results are written as `.trx` files in the specified directories. Frontend test report are written into the directory of `TestResults/Frontend/results.xml`.

## Build

Build the backend:

```sh
dotnet build backend/EnterEngine.slnx
```

Build the frontend:

```sh
cd frontend
npm run build
```

## Design decisions

- Manufacturers are stored as predefined database records. Vehicles reference them by ID.
- Vehicle weights use C# `decimal` and SQL Server `decimal(10,2)`.
- The API rejects invalid input even when requests bypass the frontend.
- Categories are calculated when vehicles are retrieved, preventing stored assignments from becoming outdated.
- Category changes are validated and saved together so neighbouring ranges can be adjusted in one operation.
- Sorting runs in the frontend because the current application loads the complete vehicle list. Server-side pagination would require sorting the full dataset in the API before selecting a page.

## Troubleshooting

### The frontend loads but API requests fail

Confirm that the backend is running on `http://localhost:5080` and that the Vite proxy targets that address.

### SQL Server login fails

Check that the connection-string password matches the SQL Server instance being used.

### Integration tests report “Set ENTERENGINE_TEST_SQL”

Set the environment variable in the same terminal used to run the tests. API User Secrets do not replace this test-specific setting.

### Port 1433 is already in use

Check whether an existing SQL Server container is running. Reuse it or stop it before starting another container on the same port.

## Deployment

GitHub Pages can host the compiled React frontend. The ASP.NET Core API and SQL Server require separate hosting.

Deployment also requires an API address accessible to the frontend, HTTPS and any necessary CORS configuration. The Vite development proxy is not included in the production build.

Keep `.env`, credentials, `node_modules` and build output out of source control. A private GitHub repository does not automatically make a deployed website private.
