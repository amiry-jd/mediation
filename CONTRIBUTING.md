# Contributing

Thanks for helping improve Mediation.

## Development

```bash
dotnet restore Mediation.slnx --locked-mode
dotnet build Mediation.slnx --configuration Release --no-restore
dotnet test tests/Mediation.Tests/Mediation.Tests.csproj --configuration Release
```

## Quality Gates

Before opening a pull request:

- Keep nullable warnings and analyzers clean.
- Add or update tests for behavior changes.
- Keep line coverage at or above 90%.
- Update `Mediation/PublicAPI.Unshipped.txt` for intentional public API additions.
- Document breaking changes in `docs/api/breaking-change-policy.md`.
- Run `dotnet list Mediation.slnx package --vulnerable --include-transitive`.
- Run package validation with `dotnet pack Mediation/Mediation.csproj --configuration Release`.

## Commit Style

Use concise conventional prefixes:

- `feat:`
- `fix:`
- `refactor:`
- `perf:`
- `test:`
- `docs:`
- `security:`
- `build:`
- `ci:`

## Pull Requests

Include a clear summary, validation results, and any public API or security impact.
