# Coverage Report

## Targets

- Line coverage: 90% or greater.
- Branch coverage: 85% target for meaningful branch-bearing code.

## Tooling

- Coverlet collector and MSBuild integration.
- ReportGenerator local tool.
- CI uploads coverage artifacts from `artifacts/coverage`.

## Local Command

```bash
dotnet test tests/Mediation.Tests/Mediation.Tests.csproj \
  --configuration Release \
  /p:CollectCoverage=true \
  /p:CoverletOutput=../../artifacts/coverage/ \
  /p:CoverletOutputFormat=cobertura \
  /p:Threshold=90 \
  /p:ThresholdType=line \
  /p:ThresholdStat=total
```

## Current Status

Coverage validation is automated. Final measured values are recorded in `docs/final-report.md` after validation.
