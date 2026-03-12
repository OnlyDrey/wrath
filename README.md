# wrath MVP foundation

## Project overview
`wrath` is a native Windows MVP for managing and launching remote access connections (starting with RDP and SSH) from a single app shell.

## Projects structure
- `src/wrath.app`: WinUI 3 app host and navigation shell
- `src/wrath.ui`: MVVM view-models and command helpers
- `src/wrath.application`: use cases and orchestration services
- `src/wrath.domain`: entities, enums, validation rules, repository interfaces
- `src/wrath.infrastructure`: SQLite initialization and repository implementations
- `src/wrath.security`: vault adapter abstraction and placeholder implementation
- `src/wrath.protocols.abstractions`: protocol contracts and dispatch
- `src/wrath.protocols.rdp`: RDP launch provider (`mstsc.exe` handoff)
- `src/wrath.protocols.ssh`: SSH launch provider (`ssh.exe` handoff)
- `tests/wrath.domain.tests`: domain rule tests
- `tests/wrath.application.tests`: application flow tests

## Setup
1. Install .NET 8 SDK.
2. Install Windows App SDK / WinUI 3 development workload.
3. Restore dependencies:
   - `dotnet restore wrath.sln`
4. Build:
   - `dotnet build wrath.sln`
5. Run tests:
   - `dotnet test wrath.sln`
6. Run app:
   - `dotnet run --project src/wrath.app/wrath.app.csproj`

## MVP capabilities
- Create and edit connection profiles
- Persist profile metadata in SQLite
- Search profiles by name/host/tags
- Launch RDP sessions through provider dispatch
- Launch SSH sessions through provider dispatch
- Record and view session launch history

## Current Implementation Status
- **Solution structure**: multi-project solution with layered boundaries (`domain`, `application`, `infrastructure`, `security`, `protocols`, `ui`, `app`).
- **Domain model**: implemented core entities (`ConnectionProfile`, `CredentialRef`, `SessionRecord`, `Tag`, `VaultPolicy`) and validation rules.
- **Application services**: implemented create, update, search, launch, and session event recording orchestration.
- **SQLite persistence**: implemented DB initialization plus repositories for profiles and session history.
- **Protocol providers**: implemented RDP and SSH providers behind a common protocol dispatch contract.
- **WinUI shell**: implemented navigation, connection list/details/edit pages, search box, and launch actions.
- **Session history**: session launch events are persisted and displayed in history page.
- **Unit tests**: domain and application tests are present for validation and launch/search flow behavior.

## Quick Verification (5-minute sanity check)
1. Build solution:
   - `dotnet build wrath.sln`
2. Run the app:
   - `dotnet run --project src/wrath.app/wrath.app.csproj`
3. In the UI, create a new profile:
   - Name: `Test RDP`
   - Protocol: `Rdp`
   - Host: `localhost`
   - Port: `3389`
4. Verify SQLite persistence:
   - Confirm database file is created under `%LOCALAPPDATA%\wrath\wrath.db`.
   - Optional: inspect `connection_profiles` rows with a SQLite viewer.
5. Search for the profile using the top search box (`Test` or `localhost`).
6. Trigger **Launch** for the RDP profile and verify `mstsc.exe` starts.
7. Create an SSH profile (`Protocol=Ssh`, `Port=22`) and trigger **Launch** to verify `ssh.exe` handoff.
8. Open **Session History** page and confirm launch requested/success or failure entries are recorded.

## Development Roadmap (Next Steps)

### Phase 1 — Stabilize MVP
- Ensure solution builds cleanly on a standard Windows dev machine
- Verify DI wiring across all runtime paths
- Improve error handling and user-visible failure states
- Add structured logging around protocol launch attempts/results
- Improve SQLite schema management (versioning/migrations)

### Phase 2 — UX improvements
- Improve connection list usability and clarity
- Expand grouping/tag management in UI workflows
- Improve search behavior and filtering controls
- Add clear launch feedback (status messages/toasts)

### Phase 3 — Security layer
- Implement real credential vault integration
- Integrate Windows Credential Manager and/or DPAPI-backed storage
- Replace placeholder vault adapter with production implementation

### Phase 4 — Protocol expansion
- Add SFTP protocol provider and file operations flow
- Add HTTPS/web session launch flow
- Introduce plugin-based provider loading model for future protocols

### Phase 5 — Enterprise features
- Add policy controls and enforcement points
- Add profile import/export workflows
- Add team profile sharing model
- Add stronger audit logging and export support

## Architecture Overview
- **Domain**: business entities, value objects, invariants, and repository contracts.
- **Application**: orchestration/use-case services that coordinate domain objects and interfaces.
- **Infrastructure**: implementations for external systems (SQLite persistence, initialization, logging hooks).
- **Security**: credential vault abstraction and secret-handling integration points.
- **Protocols**: provider contracts and concrete launch providers (RDP, SSH), selected through dispatcher.
- **UI**: WinUI shell and MVVM view-models that call application services.

Dependency direction:
- `UI -> Application -> Domain`
- `Infrastructure/Security/Protocols` implement interfaces consumed by `Application` and are wired via DI in `wrath.app`.

## Contribution Guidelines
- Follow modern C# conventions with nullable reference types enabled.
- Keep UI code thin: pages should delegate logic to view-models/services.
- Keep orchestration in the application layer (no direct infra/protocol logic in UI).
- Keep business rules and validation in the domain layer.
- Keep external system concerns (DB, OS integrations, process launch, vault providers) in infrastructure/security/protocol projects.
