# Release Process

1. Ensure `main` is green.
2. Confirm `docs/final-report.md` is up to date.
3. Move approved APIs from `PublicAPI.Unshipped.txt` to `PublicAPI.Shipped.txt`.
4. Create a semantic version tag:

```bash
git tag v1.4.2
git push origin v1.4.2
```

5. Monitor the release workflow.
6. Verify the GitHub Release and NuGet package.

Packages are not published from ordinary pushes to `main`.
