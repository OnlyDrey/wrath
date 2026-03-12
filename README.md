# wrath MVP foundation

## Projects
- `src/wrath.app`: WinUI 3 shell and composition root
- `src/wrath.ui`: MVVM view-models and command abstractions
- `src/wrath.application`: use cases and orchestrators
- `src/wrath.domain`: entities and domain rules
- `src/wrath.infrastructure`: SQLite repositories and startup init
- `src/wrath.security`: vault adapter abstraction
- `src/wrath.protocols.*`: protocol contracts + RDP/SSH providers
- `tests/*`: unit tests

## Setup
1. Install .NET 8 SDK and Windows App SDK workloads.
2. Restore: `dotnet restore wrath.sln`
3. Build: `dotnet build wrath.sln`
4. Test: `dotnet test wrath.sln`
5. Run app: `dotnet run --project src/wrath.app/wrath.app.csproj`

## MVP capabilities
- Create/edit connection profile
- Persist profiles in SQLite
- Search profiles
- Launch RDP (`mstsc.exe`) and SSH (`ssh.exe`) through provider dispatch
- Record launch session events and view recent history
