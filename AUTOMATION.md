# Autonomous Maintenance Workflow

This repository is designed for repeatable automated maintenance.

## Loop

1. Pull latest changes from `main`.
2. Inspect `git status`, recent commits, workflows, docs, and validation artifacts.
3. Review source and tests for regressions, dependency updates, and API changes.
4. Refactor only when a concrete quality, correctness, or maintainability issue is identified.
5. Add or update deterministic tests for behavior changes.
6. Run validation:
   - `dotnet restore Mediation.slnx --locked-mode`
   - `dotnet build Mediation.slnx --configuration Release --no-restore`
   - `dotnet test tests/Mediation.Tests/Mediation.Tests.csproj --configuration Release`
   - API compatibility validation
   - vulnerability validation
   - package validation
   - benchmark smoke validation
7. Diagnose failures and repeat until green.
8. Update documentation and reports.
9. Create logical conventional commits.
10. Push changes and monitor CI.

## Guardrails

- Do not duplicate existing docs, tests, workflows, or tooling.
- Do not suppress warnings unless the suppression is documented and justified.
- Do not publish packages except from version tags.
- Do not commit secrets or generated local caches.
