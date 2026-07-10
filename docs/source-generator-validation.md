# Source Generator Validation

Mediation does not currently include a source generator.

The original modernization requirement for source generator validation is therefore not applicable. If a generator is added later, create `tests/SourceGeneratorTests` and validate:

- Generated code compiles.
- Output is deterministic.
- Incremental behavior is correct.
- Duplicate outputs are prevented.
- Build regressions are caught in CI.
