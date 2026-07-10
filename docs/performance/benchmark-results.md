# Benchmark Results

## Benchmarks

BenchmarkDotNet benchmarks are implemented in `benchmarks/Mediation.Benchmarks`.

| Benchmark | Purpose |
| --- | --- |
| `SendWithoutBehaviors` | Baseline request dispatch with no pipeline behaviors. |
| `SendWithTwoBehaviors` | Dispatch with two no-op pipeline behaviors. |

## Local Command

```bash
dotnet run --configuration Release --project benchmarks/Mediation.Benchmarks/Mediation.Benchmarks.csproj -- --filter '*'
```

## CI Smoke Validation

CI runs a short BenchmarkDotNet job to catch build/runtime regressions without treating microbenchmark numbers as release gates.

## Current Results

Environment-specific throughput, memory, and allocation data are generated under `artifacts/benchmarks` during validation. The benchmark suite is implemented and ready for repeatable measurement.
