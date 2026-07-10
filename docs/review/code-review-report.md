# Code Review Report

## Scope

Reviewed public API design, naming, maintainability, extensibility, null safety, error handling, dependency management, serialization, thread safety, memory allocation, async correctness, resource disposal, and performance-sensitive paths.

## Findings and Resolutions

| Area | Finding | Resolution |
| --- | --- | --- |
| API design | Original mediator used runtime dynamic invocation. | Replaced with registry-backed typed pipeline dispatch. |
| Naming | `RequestHandlerDelegate` triggered API naming concerns. | Renamed to `RequestPipelineContinuation<TResponse>`. |
| Null safety | Public registration and dispatch APIs needed guard clauses. | Added `ArgumentNullException.ThrowIfNull` checks. |
| Error handling | Missing registrations and duplicates needed clear failures. | Added deterministic `InvalidOperationException` messages. |
| Dependency management | Runtime dependency used concrete DI package. | Runtime package references DI abstractions; tests/sample use concrete DI. |
| Serialization | No serialization is performed. | No action required. |
| Thread safety | Registry is built during configuration and read after provider creation. | Kept immutable-in-practice registration flow; duplicate registration fails fast. |
| Async correctness | Tests/sample avoided blocking `ValueTask`. | Converted blocking calls to `await`. |
| Resource disposal | Service providers/scopes are disposed in tests and samples. | Added `using` scopes where needed. |
| Performance | Dynamic dispatch and allocation-heavy reflection path were hot risks. | Dispatch now uses typed registrations and compiled delegates. |
| Public API tracking | API drift had no gate. | Added PublicAPI files and API compatibility tool. |

## Remaining Design Tradeoffs

- Behaviors are instantiated as parameterless value types, which keeps dispatch simple but excludes constructor injection.
- Assembly scanning remains reflection-based and is documented as trim/AOT sensitive.
- The registry currently allows one handler pipeline per concrete request type.

## Recommendation

Keep the current API minimal for the first release. Add source-generated registration in a future minor or major version if Native AOT support becomes a primary goal.
