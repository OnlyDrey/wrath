# wrath – Windows Remote Access Tool Hub

## 1) High-level vision

### What wrath is
**wrath** is a native Windows application for securely organizing, launching, and monitoring remote access sessions across protocols (RDP, SSH, SFTP, HTTPS, and extensible future protocols) from one cohesive workspace.

It is intentionally designed as a **Windows-first control plane** rather than a thin launcher:
- Unified connection catalog
- Protocol-aware session surfaces
- Credential and policy-aware execution
- Auditable, enterprise-ready remote workflow hub

### Who it is for
- **IT administrators** managing fleets of servers and endpoints
- **Infrastructure and platform engineers** working across Windows/Linux estates
- **Developers and SREs** needing rapid terminal/file/web access
- **Security and operations teams** requiring governed, auditable access workflows
- **Enterprises** needing policy controls, compliance, and identity integration

### Philosophy of wrath
1. **Native first**: deep Windows integration, predictable performance, accessibility, and enterprise deployment.
2. **Security as default posture**: least-privilege, encrypted secrets, explicit trust boundaries.
3. **Protocol parity**: every protocol is a first-class citizen with shared UX primitives.
4. **Operational flow over novelty**: optimize for daily high-frequency workflows.
5. **Composable architecture**: protocol engines and UI surfaces evolve independently.

---

## 2) Application architecture

### Recommended stack (clean-slate, native Windows)
- **Primary language/runtime**: **C# / .NET 8+**
- **UI framework**: **WinUI 3** (Windows App SDK)
- **Performance/native interop layer**: focused **C++/WinRT** helpers where needed (optional, bounded)
- **Data/ORM**: lightweight repository layer over **SQLite** (for non-secret metadata)
- **Serialization**: `System.Text.Json`
- **DI & hosting**: `Microsoft.Extensions.Hosting` + `Microsoft.Extensions.DependencyInjection`

This provides long-term maintainability, rapid development, and native Windows UX.

### Internal layer separation
1. **wrath.ui**
   - WinUI views, view-models, commands, navigation, theming
   - No protocol/network code directly
2. **wrath.application**
   - Use-cases/orchestration (launch session, rotate secret, import/export)
   - Enforces policy and workflow rules
3. **wrath.domain**
   - Core entities/value objects: `ConnectionProfile`, `CredentialRef`, `SessionRecord`, `Tag`, `VaultPolicy`
4. **wrath.infrastructure**
   - Persistence, vault adapters, logging sinks, telemetry, local cache
5. **wrath.protocols**
   - Protocol engine host + protocol plugins (RDP/SSH/SFTP/HTTPS)
6. **wrath.security**
   - Cryptographic services, secret broker, trust store, secure memory wrappers

### Module boundaries
- UI depends on Application contracts only.
- Application depends on Domain abstractions + interfaces.
- Infrastructure and Protocol modules implement interfaces and are injected.
- No protocol plugin may directly access UI or raw vault keys.

### Protocol engine structure
Each protocol implemented as a plugin assembly implementing:
- `IProtocolProvider`
- `IConnectionLauncher`
- `ICapabilitiesDescriptor`
- `IProtocolSettingsSchema`
- `ISessionTelemetryEmitter`

#### RDP engine
- Use **Microsoft Terminal Services Client (MSTSCLib) / native RDP APIs** through an adapter.
- Support monitor selection, dynamic resolution, clipboard/device policies.
- Policy gates for drive/printer redirection.

#### SSH engine
- Managed adapter to robust SSH library (e.g., SSH.NET) or enterprise-approved native backend.
- PTY sizing, key auth, agent forwarding policy controls, host key pinning.

#### SFTP engine
- Reuses SSH transport abstraction.
- Virtualized file tree, resumable transfer queue, checksum verification.

#### HTTPS launcher
- Embedded hardened web surface (WebView2 in isolated profile) or system browser handoff by policy.
- Per-profile certificate pinning, mutual TLS options.

### Plugin architecture for future protocols
- Signed plugin manifests (`plugin.json`) + assembly signature validation.
- Capabilities negotiated at load time.
- Sandboxed execution boundary using out-of-process workers for untrusted/third-party plugins.
- Versioned plugin API (`v1`, `v2`) with compatibility contract tests.

### Settings, themes, profiles, secure storage
- **Global settings**: app behavior, defaults, accessibility profile.
- **Workspace settings**: team/imported bundles and policy overlays.
- **Profile settings**: per-connection protocol settings.
- **Theme system**: Light, Dark, High Contrast with tokenized design system.
- **Secrets**: never stored in plain DB; use Windows credential vault + DPAPI-protected envelopes for app-managed secrets.

---

## 3) UI/UX design proposals (5 complete concepts)

## Concept A — Command Center
**Ideal for:** admins managing many hosts.
- **Layout**: left nav rail + center dashboard + right contextual inspector.
- **Navigation**: Dashboard, Connections, Sessions, Transfers, Audit, Settings.
- **Dashboard**: KPI tiles (active sessions, failed auths, recent hosts), quick-launch chips.
- **Connection list**: dense table with status dots, tags, environment columns.
- **Detail view**: tabs (Overview, Auth, Network, Advanced, History).
- **Sessions**: tabbed surface with optional split panes.
- **Visual style**: Fluent cards, subtle Mica background, semantic status colors.

## Concept B — Workspace Explorer
**Ideal for:** engineers who think in environment trees.
- **Layout**: folder tree left, list center, preview/detail right.
- **Navigation**: breadcrumb + command bar.
- **Dashboard**: environment map (Prod/Staging/Dev).
- **Connection list**: grouped by folder/project.
- **Detail view**: editable form-first surface with live validation.
- **Split options**: vertical split for terminal + file browser.
- **Visual style**: acrylic side panes, compact typography.

## Concept C — Session-First Pro Console
**Ideal for:** power users rapidly switching active sessions.
- **Layout**: top tab strip (session tabs), collapsible left quick-switcher.
- **Navigation**: command palette (`Ctrl+K`) and keyboard-first actions.
- **Dashboard**: minimal “resume recent sessions” strip.
- **Connection list**: quick filter chips + fuzzy search.
- **Detail view**: modal or side sheet to reduce context switching.
- **Split options**: 2x2 session grid for concurrent monitoring.
- **Visual style**: darker default, high-information density.

## Concept D — Operations Wall
**Ideal for:** NOC/SOC and monitoring-heavy workflows.
- **Layout**: customizable widgets with live session telemetry.
- **Navigation**: role-based presets (Operator, Admin, Auditor).
- **Dashboard**: alerts timeline, failed connection heatmap.
- **Connection list**: SLA/SLO columns and policy compliance badges.
- **Detail view**: includes audit timeline and trust posture panel.
- **Split options**: pinned “watch sessions” grid.
- **Visual style**: high-contrast status-led UI, large readable typography.

## Concept E — Guided Access Studio
**Ideal for:** mixed-skill teams needing structured flows.
- **Layout**: wizard-enhanced authoring + standard runtime views.
- **Navigation**: task-based (Create, Validate, Launch, Share).
- **Dashboard**: “Recommended actions” cards.
- **Connection list**: beginner/pro toggle (basic vs advanced fields).
- **Detail view**: progressive disclosure for advanced protocol options.
- **Split options**: side-by-side “config” and “test run” panel.
- **Visual style**: approachable, larger spacing, educational inline hints.

### Shared visual system guidance (for Figma)
- **Grid**: 8px base spacing; container widths at 320/640/960/1280.
- **Typography**: Segoe UI Variable; hierarchy (12/14/16/20/28).
- **Icons**: Fluent System Icons, protocol glyph overlays (terminal, monitor, folder, globe, lock).
- **Color themes**:
  - Light: neutral grays + cobalt accent
  - Dark: near-black surfaces + teal/cyan highlights
  - High Contrast: WCAG AAA tokens with explicit focus outlines
- **Windows-native patterns**: WinUI command bars, teaching tips, content dialogs, navigation view, Mica on shell surfaces, Acrylic only on transient panes.

---

## 4) Feature set

### Core features
1. **RDP integrations**
   - Saved display presets, multi-monitor mapping, device redirection controls.
2. **SSH terminal**
   - Multiple tabs, profile-based shells, host key verification UX.
3. **SFTP browser**
   - Dual-pane transfer UI, queued uploads/downloads, resume and conflict policies.
4. **HTTPS launchers**
   - Open internal tools with per-host cert requirements and SSO policy hooks.
5. **Organization model**
   - Folders/groups, nested environments, smart groups via query rules.
6. **Favorites and tags**
   - Starred quick access; tags for team/system/criticality.
7. **Search**
   - Instant global search across names, hosts, tags, notes, recent history.
8. **Per-host credentials in secure vault**
   - Link profiles to vault entries and key material references.
9. **Connection history**
   - Timeline by host/user/protocol/outcome.
10. **Logging and diagnostics**
   - Structured logs, session event traces, redacted error exports.
11. **Import/export**
   - Encrypted bundle formats for backup and team migration.
12. **Passwordless & cert auth**
   - Passkeys/WebAuthn, smartcards/certificates, SSH keys with hardware token support.
13. **Multi-monitor RDP awareness**
   - Detect monitor topology, select subsets, remember layouts per host.

### Suggested premium/enterprise roadmap
- Central policy server and role-based access control
- Just-in-time access approvals and session recording metadata
- Privileged access workflow integration (PAM/PIM)
- SIEM streaming connectors (Splunk, Sentinel, Elastic)
- Team workspaces with delegated administration
- Compliance packs (SOX, HIPAA, ISO audit templates)
- Fleet inventory sync from CMDB/MDM sources

---

## 5) Security model

### Encryption at rest
- Metadata in SQLite encrypted with app key hierarchy.
- Secrets encrypted using DPAPI (user or machine scope by policy).
- Optional enterprise mode with key wrapping via Windows CNG/KSP-backed keys.

### OS key store usage
- Use **Windows Credential Manager / PasswordVault** for credential references.
- Certificates/keys in Windows certificate store and TPM-backed providers where available.

### Credential isolation
- Main app process never exposes raw secret material to UI binding layer.
- Secret broker service returns short-lived tokens/handles, not plaintext.
- Session workers receive only protocol-specific ephemeral credentials.

### Secrets handling
- Zeroization of secure buffers after use.
- Clipboard blocking option for sensitive fields.
- No secrets in logs, dumps, or telemetry.

### Hardening against injection/tampering
- Enforce code signing and plugin signature verification.
- Optional process mitigation policies (CFG, DEP, ASLR, dynamic code restrictions where feasible).
- Integrity checks on startup and plugin load.

### Secure auto-fill
- Explicit user gesture required for credential fill in remote forms.
- Time-bound autofill session with visual indicator and revoke action.

### Audit logging
- Immutable local audit trail with hash-chained entries.
- Export signed audit bundles for compliance review.

### Zero-trust recommendations
- Default deny unknown host keys/certificates.
- Require explicit trust bootstrap flow.
- Per-connection least privilege templates.
- Network segmentation awareness and conditional access integration.

---

## 6) Windows-native technology choices

- **UI framework**: WinUI 3 + Windows App SDK
- **Process management**: .NET generic host + isolated worker processes for protocol execution
- **Protocol handling**:
  - RDP: MSTSC ActiveX/native RDP APIs wrapped behind interface
  - SSH/SFTP: managed SSH library abstraction with strict host-key policy
  - HTTPS: `HttpClient` + `SocketsHttpHandler` + custom cert validation callbacks
- **Crypto/network libraries**:
  - `System.Security.Cryptography` (CNG-backed)
  - Windows cryptographic APIs for key storage and cert operations
- **Windows APIs**:
  - `Windows.Security.Credentials` (vault integration)
  - `Windows.Security.Cryptography`
  - `Windows.System` for protocol activation and launcher integration
  - `Windows.UI.ViewManagement` / modern equivalents for display-awareness logic
- **Packaging**: **MSIX** for enterprise-friendly deployment, signing, and rollback
- **Auto-update strategy**:
  - Store-distributed channel for public builds
  - Enterprise offline update channel via signed MSIX bundles + update service endpoint

---

## 7) Recommended repository structure

```text
wrath/
├─ src/
│  ├─ wrath.app/                    # WinUI app bootstrap
│  ├─ wrath.ui/                     # Views, view-models, theming, resources
│  ├─ wrath.application/            # Use-cases, orchestration, DTOs
│  ├─ wrath.domain/                 # Core entities, policies, contracts
│  ├─ wrath.infrastructure/         # Persistence, vault adapters, logging
│  ├─ wrath.security/               # Crypto, secret broker, trust services
│  ├─ wrath.protocols/
│  │  ├─ wrath.protocols.abstractions/
│  │  ├─ wrath.protocols.host/
│  │  ├─ wrath.protocols.rdp/
│  │  ├─ wrath.protocols.ssh/
│  │  ├─ wrath.protocols.sftp/
│  │  └─ wrath.protocols.https/
│  └─ wrath.shared/                 # Shared primitives and utilities
├─ assets/
│  ├─ branding/
│  ├─ icons/
│  ├─ themes/
│  └─ templates/
├─ docs/
│  ├─ architecture/
│  ├─ security/
│  ├─ ux/
│  ├─ protocols/
│  └─ operations/
├─ tests/
│  ├─ wrath.unit/
│  ├─ wrath.integration/
│  ├─ wrath.security/
│  └─ wrath.ui.automation/
├─ build/
│  ├─ scripts/
│  ├─ pipelines/
│  └─ packaging/
└─ tools/
   ├─ analyzers/
   └─ devcontainer/
```

---

## 8) Branding and identity

### Logo concepts
1. **Shield + Node Grid**: security + connection hub motif.
2. **Angular W Monogram**: sharp “W” constructed from linked path lines.
3. **Portal Frame**: minimal window frame with directional ingress arrow.

### Icon design
- Primary app icon: monochrome geometric “W” on layered gradient plate.
- Secondary protocol badges: monitor (RDP), terminal prompt (SSH), folder-arrows (SFTP), globe-lock (HTTPS).

### Color palettes
- **Core palette**: Cobalt (#2F5BFF), Graphite (#1F232B), Cloud (#F4F7FB)
- **Dark palette**: Night (#0D1117), Slate (#161B22), Cyan accent (#22D3EE)
- **Alert/status**: Success (#22C55E), Warning (#F59E0B), Critical (#EF4444)

### Splash screen ideas
- Subtle animated connection lines forming the “W” symbol.
- Security statement and build channel shown during startup.

### Taglines
- “One hub. Every remote path.”
- “Secure access, engineered for Windows.”
- “Control remote infrastructure with confidence.”

### Tone of voice
- Clear, direct, operations-focused.
- Explain *why* for security decisions.
- Avoid hype; prioritize trust, precision, and reproducibility.

### Naming conventions
- **wrath Core** (domain + orchestration)
- **wrath Connectors** (protocol plugins)
- **wrath Vault Bridge** (credentials integration)
- **wrath Policy Pack** (enterprise governance add-on)

---

## 9) Developer experience

### Coding standards
- C# analyzers enabled (`TreatWarningsAsErrors=true` for core projects).
- SOLID boundaries and explicit interface contracts.
- Nullable reference types enabled everywhere.
- Public APIs fully XML-documented.

### Documentation structure
- `docs/architecture/` for diagrams, decisions (ADR format)
- `docs/security/` threat models and cryptographic decisions
- `docs/protocols/` protocol behavior contracts
- `docs/operations/` deployment, logging, incident guidance

### Testing strategy
- **Unit tests**: domain rules, mappers, policy evaluation.
- **Integration tests**: protocol adapters with mocked endpoints.
- **Security tests**: secret lifecycle, redaction validation, cert pinning behavior.
- **UI automation**: critical user journeys and accessibility checks.

### Linting/formatting
- `dotnet format`
- Roslyn analyzers + StyleCop (if adopted)
- Pre-commit hooks for formatting and baseline checks

### Build pipelines
- CI stages: restore → build → test → security scans → package MSIX → sign artifacts.
- Nightly builds generate canary channel packages.

### Versioning
- Semantic versioning for app (`MAJOR.MINOR.PATCH`).
- Plugin API has independent compatibility version.

### Release notes strategy
- User-facing highlights + admin/security impact section.
- Explicit migration notes for policy or plugin API changes.
- CVE/security advisories linked when relevant.

---

## 10) Sample code snippets (recommended stack: C#/.NET)

> Note: snippets illustrate architecture and API shape for the first implementation pass.

### A) Launching an RDP session (shell handoff)
```csharp
using System.Diagnostics;

public static class RdpLauncher
{
    public static void Launch(string host, string? username = null)
    {
        var args = username is null
            ? $"/v:{host}"
            : $"/v:{host} /u:{username}";

        Process.Start(new ProcessStartInfo
        {
            FileName = "mstsc.exe",
            Arguments = args,
            UseShellExecute = false
        });
    }
}
```

### B) Opening an SSH connection (SSH.NET)
```csharp
using Renci.SshNet;

public sealed class SshSession : IDisposable
{
    private readonly SshClient _client;

    public SshSession(string host, string user, PrivateKeyFile key)
    {
        _client = new SshClient(host, user, key);
    }

    public string Run(string command)
    {
        _client.Connect();
        var result = _client.RunCommand(command).Result;
        _client.Disconnect();
        return result;
    }

    public void Dispose() => _client.Dispose();
}
```

### C) SFTP directory listing
```csharp
using Renci.SshNet;

public static class SftpExample
{
    public static IReadOnlyList<string> ListDirectory(
        string host,
        string user,
        PrivateKeyFile key,
        string path)
    {
        using var client = new SftpClient(host, user, key);
        client.Connect();

        var entries = client.ListDirectory(path)
            .Where(e => e.Name is not "." and not "..")
            .Select(e => e.FullName)
            .ToList();

        client.Disconnect();
        return entries;
    }
}
```

### D) HTTPS connection with certificate pinning
```csharp
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

public static class HttpsPinnedClient
{
    // SHA-256 hash of expected certificate public key (SPKI) bytes.
    private static readonly byte[] ExpectedSpkiHash = Convert.FromHexString(
        "0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF");

    public static async Task<string> GetAsync(Uri uri, CancellationToken ct)
    {
        var handler = new SocketsHttpHandler
        {
            SslOptions =
            {
                RemoteCertificateValidationCallback = (_, cert, _, errors) =>
                {
                    if (errors != System.Net.Security.SslPolicyErrors.None || cert is null)
                        return false;

                    var x509 = new X509Certificate2(cert);
                    var pubKey = x509.PublicKey.EncodedKeyValue.RawData;
                    var hash = SHA256.HashData(pubKey);
                    return CryptographicOperations.FixedTimeEquals(hash, ExpectedSpkiHash);
                }
            }
        };

        using var http = new HttpClient(handler, disposeHandler: true);
        return await http.GetStringAsync(uri, ct);
    }
}
```

---

## Closing implementation guidance
Ship wrath in staged milestones:
1. **MVP**: profiles, vault-backed credentials, RDP + SSH, search, basic history.
2. **Ops-ready**: SFTP, HTTPS launcher, auditing, import/export, policy templates.
3. **Enterprise**: centralized policy, approval workflows, SIEM integration, compliance packs.

This sequencing keeps time-to-value high while building the secure foundation first.
