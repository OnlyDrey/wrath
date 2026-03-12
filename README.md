# wrath

wrath is a native Windows remote access hub built with .NET 8 and WinUI 3. It provides a single place to manage connection profiles, launch supported protocols, and track session launch history while keeping domain, application, and infrastructure responsibilities clearly separated.

## Why this project exists

Windows administrators and engineers often switch between separate tools for RDP, SSH, file access, and operational tracking. This project exists to provide one Windows-first application that centralizes remote connection management, protocol launch orchestration, and session traceability.

## Key Features

- Connection profiles with protocol, host, port, user, grouping, and tags
- Protocol abstraction with provider dispatch
- RDP and SSH launch providers
- Session history recording for launch attempts and outcomes
- SQLite persistence for profile/session metadata
- Layered architecture (domain/application/infrastructure/security/protocols/UI)

## Screenshots

Screenshots will be added as the UI matures.

## Architecture

- **Domain**: entities, enums, value objects, and validation rules
- **Application**: use-case orchestration and service workflows
- **Infrastructure**: SQLite initialization, migrations, and repositories
- **Security**: credential vault abstraction and integration point
- **Protocols**: provider contracts and concrete launch handlers (RDP/SSH)
- **UI**: WinUI shell, pages, and MVVM view-models

Dependency direction:

- `UI -> Application -> Domain`
- `Infrastructure`, `Security`, and `Protocols` implement interfaces used by `Application` and are wired in the app composition root.

## Project Structure

- `src/wrath.app` — WinUI host, navigation shell, DI composition root
- `src/wrath.ui` — ViewModels and MVVM command primitives
- `src/wrath.application` — Profile/session use-case services
- `src/wrath.domain` — Core model and business rules
- `src/wrath.infrastructure` — SQLite repositories and schema migration runner
- `src/wrath.security` — Vault abstraction and current placeholder adapter
- `src/wrath.protocols.abstractions` — Protocol contracts + dispatcher
- `src/wrath.protocols.rdp` — `mstsc.exe` handoff provider
- `src/wrath.protocols.ssh` — `ssh.exe` handoff provider

## Setup

Requirements:

- Windows 10/11
- .NET 8 SDK
- WinUI 3 / Windows App SDK development tooling

Commands:

- `dotnet restore wrath.sln`
- `dotnet build wrath.sln`
- `dotnet test wrath.sln`
- `dotnet run --project src/wrath.app/wrath.app.csproj`

## Quick Verification (5 minute test)

1. Build and run the app.
2. Create a new RDP profile (`localhost`, port `3389`, tags like `lab,windows`).
3. Confirm the profile appears in the Connections list.
4. Search by name, host, and tag to confirm all match paths work.
5. Launch the RDP profile and verify handoff to `mstsc.exe`.
6. Create an SSH profile (`localhost`, port `22`) and launch it.
7. Open Session History and verify launch requested + success/failure records are present.
8. Confirm SQLite file creation at `%LOCALAPPDATA%\wrath\wrath.db`.

## Development Roadmap

- **Phase 1 – Stabilize MVP**: build reliability, diagnostics, migration coverage, and failure handling
- **Phase 2 – UX improvements**: better connection management workflows and launch feedback
- **Phase 3 – Security / credential vault**: production vault integration and secret handling
- **Phase 4 – Protocol expansion**: SFTP/HTTPS and plugin-ready provider growth
- **Phase 5 – Enterprise capabilities**: policy controls, import/export, sharing, and audit depth

## Contributing

Please keep changes aligned to layer responsibilities:

- Keep UI thin and focused on presentation/state binding
- Keep business rules and validation in the domain layer
- Keep orchestration and use-case flows in the application layer
- Keep external integrations (database, OS process, vault adapters) in infrastructure/security/protocol projects
