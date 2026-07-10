# Release Guide

## Local Development

```bash
dotnet restore Mediation.slnx --locked-mode
dotnet build Mediation.slnx --configuration Release --no-restore
dotnet test tests/Mediation.Tests/Mediation.Tests.csproj --configuration Release
```

## Benchmarking

```bash
dotnet run --configuration Release --project benchmarks/Mediation.Benchmarks/Mediation.Benchmarks.csproj -- --filter '*'
```

Review throughput and allocation columns. Do not trade maintainability for small micro-optimizations.

## Packaging

```bash
dotnet pack Mediation/Mediation.csproj --configuration Release --output artifacts/packages
```

Inspect the generated `.nupkg` for `README.md`, `mediation-icon.png`, XML docs, SourceLink metadata, and symbols.

## Publishing

Releases are published only from semantic version tags:

```bash
git tag v1.4.2
git push origin v1.4.2
```

The release workflow validates, packs, creates a GitHub Release, and publishes to NuGet.

## Required Secrets

### `NUGET_API_KEY`

1. Sign in to nuget.org.
2. Open API Keys.
3. Create a key scoped to the `Mediation` package with push permissions.
4. Add it to the GitHub repository under Settings → Secrets and variables → Actions.

### `GITHUB_TOKEN`

GitHub automatically provides `GITHUB_TOKEN` to workflows. No manual secret creation is required for release creation unless repository policy overrides default permissions.
