# Security Notes (MVP)

- No plaintext secret storage is implemented.
- Credential handling is abstracted behind `ICredentialVaultAdapter`.
- `WindowsVaultAdapter` is a safe extension point and must be replaced with Windows credential APIs before production.
- Protocol launchers only receive host/user metadata from profile records.
- Session logs intentionally contain operational events and messages only.

Future hardening:
- Integrate Windows Credential Manager / PasswordVault.
- Encrypt metadata at rest.
- Add signed audit trails and plugin signature validation.
