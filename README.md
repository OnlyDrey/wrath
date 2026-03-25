![wrath logo](assets/branding/wrath-logo-1024.png)

# wrath

**Windows Remote Access Tool Hub**

wrath is a native Windows (.NET 8 + WinUI 3) remote access hub that centralizes connection profiles, protocol launch, and session history with a layered architecture designed for incremental MVP development.

## Why this project exists

Daily remote operations on Windows usually involve multiple disconnected tools for RDP, SSH, profile management, and launch tracking. wrath provides one Windows-first workspace that keeps those workflows together while staying cleanly structured for long-term maintenance.

## Key Features

- Connection profiles (name, host, port, protocol, group, tags)
- Protocol abstraction and provider dispatch
- RDP and SSH launch providers
- Session history with launch outcomes
- SQLite persistence with schema version metadata
- Layered architecture (Domain/Application/Infrastructure/Security/Protocols/UI)

## Architecture

- **Domain**: core entities, validation, and contracts
- **Application**: orchestration services and operation results
- **Infrastructure**: SQLite initialization/migrations and repositories
- **Security**: vault abstraction and registration point
- **Protocols**: protocol contracts and provider implementations
- **UI**: WinUI shell + MVVM view models/pages

Dependency direction:

- `UI -> Application -> Domain`
- `Infrastructure`, `Protocols`, and `Security` implement interfaces consumed by `Application` and are wired in `wrath.app`.

## Project Structure

- `src/wrath.app` — WinUI host, composition root, shell pages
- `src/wrath.ui` — MVVM primitives + view models
- `src/wrath.application` — use-case services
- `src/wrath.domain` — domain model and rules
- `src/wrath.infrastructure` — SQLite persistence and migration runner
- `src/wrath.security` — credential vault abstraction
- `src/wrath.protocols.abstractions` — protocol contracts and dispatcher
- `src/wrath.protocols.rdp` — `mstsc.exe` launch provider
- `src/wrath.protocols.ssh` — `ssh.exe` launch provider
- `tests/*` — domain and application tests
- `assets/branding` — logo and brand docs
- `assets/icons` — icon export targets

## Setup

Requirements:

- Windows 10 (version 2004 / build 19041) or Windows 11
- .NET 8 SDK (`dotnet --info`)
- Windows App Runtime 1.8 installed (required for normal local framework-dependent unpackaged execution)
- Visual Studio 2022 17.8+ with Windows development tooling (recommended for WinUI diagnostics)

Local build and run (unpackaged WinUI desktop app):

1. `dotnet clean wrath.sln`
2. `dotnet restore wrath.sln`
3. `dotnet build wrath.sln`
4. `dotnet run --project src/wrath.app/wrath.app.csproj`

Optional self-contained publish (explicit RID required):

- `dotnet publish src/wrath.app/wrath.app.csproj -c Release -f net8.0-windows10.0.19041.0 -r win-x64 -p:WindowsPackageType=None -p:WindowsAppSDKSelfContained=true`

Notes:

- `src/wrath.app` is configured as unpackaged for local development (`AppxPackage=false`, `WindowsPackageType=None`).
- Do not enable `WindowsAppSDKSelfContained=true` unconditionally in the project for normal solution builds.

## Quick Verification

1. Build and run the app.
2. Create a profile (RDP, `localhost`, port `3389`, tags: `lab,windows`).
3. Confirm it appears in Connections.
4. Search by name, host, and tag.
5. Launch the RDP profile (`mstsc.exe` handoff).
6. Create and launch an SSH profile (`ssh.exe` handoff).
7. Open Session History and verify requested/success/failure entries.
8. Confirm `%LOCALAPPDATA%\wrath\wrath.db` exists.

## Current Implementation Status

- Build blocker in `VaultPolicy` named arguments corrected.
- WinUI shell includes a dark-first branding pass (header, subtitle, color tokens, styled primary actions).
- Connection create/edit, search, launch orchestration, and session recording remain intact.
- SQLite migration bootstrap includes schema metadata (`DatabaseMetadata.SchemaVersion`).

## Development Roadmap

- **Phase 1 – Stabilize MVP**: build reliability, diagnostics, DB migration coverage
- **Phase 2 – UX improvements**: denser list controls, better edit/search UX
- **Phase 3 – Security / credential vault**: concrete Windows credential integration
- **Phase 4 – Protocol expansion**: SFTP/HTTPS providers and plugin growth
- **Phase 5 – Enterprise capabilities**: policy controls, import/export, audit depth

## Branding

Primary colors:

- Electric Blue `#0078D4`
- Deep Tech Blue `#1F6FEB`
- Sunset Orange `#FF6A3D`
- Graphite `#111111`
- Background Dark `#0A0F18`
- Surface `#121A26`
- Border `#223045`
- Text Primary `#E6EDF3`
- Text Secondary `#8B949E`

Logo usage rules:

- Use the provided logo as the primary mark.
- Do not rotate the logo.
- Do not alter gradients.
- Do not add shadow effects.

Theme/type direction:

- Dark-first, minimal, Windows-native presentation.
- Segoe UI Variable for app shell UX.
- Cascadia Code is preferred for documentation-oriented brand treatments.

Icon/export targets:

- `assets/branding/wrath-logo-1024.png`
- `assets/branding/wrath-logo-512.png`
- `assets/branding/wrath-logo-256.png`
- `assets/branding/wrath-logo-128.png`
- `assets/icons/wrath-logo-64.png`
- `assets/icons/wrath-logo-32.png`
- `assets/icons/wrath-logo-16.png`
- `assets/icons/wrath.ico`

## Contributing

- Keep UI thin (presentation/state only).
- Keep domain rules in `wrath.domain`.
- Keep orchestration and error shaping in `wrath.application`.
- Keep external system integration in `wrath.infrastructure`, `wrath.protocols.*`, and `wrath.security`.
