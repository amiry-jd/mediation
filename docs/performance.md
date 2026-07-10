# Performance

Mediation avoids hot-path `DynamicInvoke` and uses typed pipeline registrations.

## Benchmark Suite

Benchmarks live in `benchmarks/Mediation.Benchmarks` and cover:

- Direct request dispatch.
- Dispatch through two no-op pipeline behaviors.

## Run

```bash
dotnet run --configuration Release --project benchmarks/Mediation.Benchmarks/Mediation.Benchmarks.csproj -- --filter '*'
```

## Guidance

Optimize only when benchmark data shows a meaningful bottleneck. Preserve clear API behavior over micro-optimizations.
