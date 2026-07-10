# Security Review

## Summary

Mediation is a local in-process dispatch library. It does not parse untrusted input, deserialize payloads, access the filesystem, execute commands, open sockets, or manage secrets.

## Reviewed Areas

| Area | Status |
| --- | --- |
| Dependency vulnerabilities | Automated with `dotnet list package --vulnerable --include-transitive`. |
| Unsafe deserialization | Not applicable; no serialization is performed. |
| Reflection abuse | Assembly scanning is limited to caller-supplied assemblies. |
| Secrets exposure | No secrets are read or stored. |
| Path traversal | Not applicable; no path input is processed. |
| Injection vulnerabilities | Not applicable; no database, shell, or network calls are made. |
| Supply chain | Mitigated by lock files, Dependabot, CodeQL, and CI gates. |

## Risks

- Consumers should not call `RegisterForAssemblies` on untrusted dynamically loaded assemblies.
- Reflection-based scanning is not suitable for all trimmed or Native AOT applications.

## Automation

- `ci.yml` runs dependency vulnerability validation.
- `codeql.yml` runs security and quality analysis.
- `dependabot.yml` checks NuGet and GitHub Actions weekly.

## Result

No repository-level security defects are known at this time.
