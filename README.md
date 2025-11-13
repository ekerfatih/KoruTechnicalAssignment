# Koru Technical Assignment

Blazor WebAssembly + Minimal API sample that implements the full appointment flow. Users create/send requests, admins list pending ones and approve or reject them, while an audit trail records every status change. The solution follows a layered architecture (Domain → Application → Infrastructure → Web) and uses FluentValidation everywhere.

## Feature Highlights
- **User portal** – status/date/search filters, pagination, sorting (date/status) and badge-style statuses. Request history appears in a Bootstrap modal.
- **Request form** – branch dropdown (5 seeded branches), date/time validation, draft vs pending behaviour, read-only mode for non-drafts.
- **Admin portal** – pending request list with filters, sorting, inline rejection reason and audit modal.
- **Global errors & notifications** – API middleware returns `ProblemDetails`; the client shows toast/snackbar messages through `NotificationService`.

## Local Setup
1. **Requirements**
   - .NET 8 SDK
   - PostgreSQL 15+ (local installation or Docker)
2. **Restore dependencies**
   ```bash
   dotnet restore src/KoruTechnicalAssignment.sln
   ```
3. **Ensure PostgreSQL is running (two databases)**
   Example via Docker:
   ```bash
   docker run -d --name koru-postgres \
     -e POSTGRES_USER=koru \
     -e POSTGRES_PASSWORD=koruPass! \
     -p 5432:5432 postgres:15
   ```
   Then create the two databases the solution expects:
   ```bash
   docker exec -it koru-postgres psql -U koru -c 'CREATE DATABASE "KoruTechDb";'
   docker exec -it koru-postgres psql -U koru -c 'CREATE DATABASE "KoruIdentityDb";'
   ```
   Update `appsettings.json` if you use different credentials/host.
4. **Run the web project**
   ```bash
   dotnet run --project src/KoruTechnicalAssignment.Web/KoruTechnicalAssignment.Web.csproj
   ```
   Default URL: `https://localhost:7122/`

> `ConnectionStrings:DefaultConnection` targets the application data database, while `ConnectionStrings:IdentityConnection` is used by ASP.NET Identity. Point both to PostgreSQL instances that are reachable from the web project.

> The application automatically creates the schema and seeds data on startup; no manual migrations are required in this branch.

## Docker Setup
1. Ensure Docker Desktop/Engine is running.
2. From the repo root:
   ```bash
   docker compose up --build
   ```
3. Navigate to `http://localhost:8080/`.
   - `postgres` container uses user `koru` / password `koruPass!`. An init script under `docker/postgres-init` creates both `KoruTechDb` and `KoruIdentityDb` the first time the volume is initialized.
   - `web` container reads `ConnectionStrings__DefaultConnection` + `ConnectionStrings__IdentityConnection` from environment variables and exposes port 8080.
4. Shut down with:
   ```bash
   docker compose down
   ```
   Data persists in the `postgres_data` volume.

## Credentials (seeded)

| Role  | Email             | Password    |
|-------|-------------------|-------------|
| Admin | admin1@koru.local | `Admin123$` |
| Admin | admin2@koru.local | `Admin123$` |
| User  | user1@koru.local  | `User123$`  |
| User  | user2@koru.local  | `User123$`  |

## Architecture Notes
- **Domain** – entities (`Request`, `Branch`, `RequestStatusHistory`, …) and enums (`RequestStatus`, `RequestSortField`, `SortDirection`).
- **Application** – repository/service contracts plus implementations (`RequestService`, `BranchService`, etc.) and FluentValidation validators.
- **Infrastructure** – EF Core DbContexts (Identity + Application) configured with Npgsql/PostgreSQL, plus seeding and repository implementations.
- **Web** – Minimal API endpoints + Blazor WASM frontend with global exception middleware returning `ProblemDetails`.
- **Audit Trail & Notifications** – `RequestHistoryModal` displays “who/when/why”; `NotificationService`/`NotificationHost` render toast feedback.

> Need a different host/port? Override `AppBaseUrl`, `ASPNETCORE_URLS` or the connection string env vars (e.g., in Docker Compose).
