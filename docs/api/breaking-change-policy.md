# Breaking Change Policy

## What Counts as Breaking

The CI API gate prevents accidental:

- Removed public types or members.
- Changed method signatures.
- Changed generic parameters or constraints.
- Changed nullability annotations.
- Changed visibility.
- Removed extension methods.

## Approval Workflow

Breaking changes require:

1. A major version plan.
2. Documentation in release notes.
3. Updates to `Mediation/PublicAPI.Shipped.txt`.
4. Maintainer approval in pull request review.

## CI Enforcement

`tools/ApiCompat` compares the current public surface against `PublicAPI.Shipped.txt` and `PublicAPI.Unshipped.txt`. CI fails when breaking changes or unapproved additions are detected.

## Exceptions

No exceptions are allowed without explicit maintainer approval and release documentation.
