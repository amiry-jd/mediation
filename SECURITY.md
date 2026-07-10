# Security Policy

## Supported Versions

Security fixes are provided for the latest released minor version.

## Reporting a Vulnerability

Please report suspected vulnerabilities privately through GitHub Security Advisories when available. If advisories are unavailable, contact the repository owner directly and avoid publishing exploit details until a fix is available.

Include:

- Affected version or commit.
- Reproduction steps or proof of concept.
- Impact assessment.
- Suggested mitigation, if known.

## Security Practices

- Dependency vulnerability scans run in CI.
- CodeQL scans run on push, pull request, schedule, and manual dispatch.
- Dependabot checks NuGet and GitHub Actions dependencies weekly.
- Secrets must never be committed.
