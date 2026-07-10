# API Compatibility

Mediation tracks public APIs with `Microsoft.CodeAnalysis.PublicApiAnalyzers` and repository-level validation.

## Baseline Files

- `Mediation/PublicAPI.Shipped.txt` contains approved shipped APIs.
- `Mediation/PublicAPI.Unshipped.txt` contains APIs proposed for the next release.

## Local Validation

```bash
dotnet build Mediation/Mediation.csproj --configuration Release
dotnet run --configuration Release --project tools/ApiCompat/ApiCompat.csproj -- ./ 
```

The validator writes `artifacts/api/Mediation.PublicAPI.Current.txt` and fails when:

- A baseline member is missing.
- A new public member exists without approval.

## API Review Process

1. Add or change code.
2. Build and review PublicApiAnalyzer diagnostics.
3. Add intentional new APIs to `PublicAPI.Unshipped.txt`.
4. Document behavior and compatibility impact.
5. During release, move approved unshipped APIs to `PublicAPI.Shipped.txt`.

## Compatibility Status

API compatibility tracking is implemented and enforced in CI.
