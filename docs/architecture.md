# Architecture

Layering:
- UI (`wrath.app`, `wrath.ui`) depends on application contracts only.
- Application (`wrath.application`) depends on domain interfaces and protocol dispatch abstraction.
- Infrastructure implements domain repositories using SQLite.
- Protocol providers implement `IProtocolProvider` and are selected by `ProtocolDispatchService`.

Vertical slice (launch):
1. User selects profile in UI.
2. `MainShellViewModel` calls `SessionService.LaunchSessionAsync`.
3. Service records `LaunchRequested` event.
4. Dispatches to protocol provider.
5. Records success/failure event.

Persistence:
- `connection_profiles`
- `session_history`

Startup:
- Host + DI configured in `App.xaml.cs`.
- `SqliteInitializer` creates tables at launch.
