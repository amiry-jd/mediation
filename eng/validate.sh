#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
artifacts="$repo_root/artifacts"
test_results="$artifacts/test-results"
packages="$artifacts/packages"
coverage="$artifacts/coverage"
coverage_file="$coverage/coverage.cobertura.xml"
skip_benchmarks="${SKIP_BENCHMARKS:-false}"

mkdir -p "$artifacts" "$test_results" "$packages" "$coverage"

dotnet restore "$repo_root/Mediation.slnx" --locked-mode
dotnet build "$repo_root/Mediation.slnx" --configuration Release --no-restore
dotnet test "$repo_root/tests/Mediation.Tests/Mediation.Tests.csproj" --configuration Release --no-build --logger "trx;LogFileName=tests.trx" --results-directory "$test_results" /p:CollectCoverage=true /p:CoverletOutput="$coverage/" /p:CoverletOutputFormat=cobertura /p:Threshold=90 /p:ThresholdType=line /p:ThresholdStat=total
dotnet tool restore
dotnet reportgenerator -reports:"$coverage_file" -targetdir:"$coverage/report" -reporttypes:"HtmlInline_AzurePipelines;Cobertura;MarkdownSummaryGithub"
dotnet run --configuration Release --project "$repo_root/tools/ApiCompat/ApiCompat.csproj" --no-build -- "$repo_root"
dotnet list "$repo_root/Mediation.slnx" package --vulnerable --include-transitive
dotnet pack "$repo_root/Mediation/Mediation.csproj" --configuration Release --no-build --output "$packages"

if [[ "$skip_benchmarks" != "true" ]]; then
  dotnet run --configuration Release --project "$repo_root/benchmarks/Mediation.Benchmarks/Mediation.Benchmarks.csproj" -- --filter '*' --job short --artifacts "$artifacts/benchmarks"
fi
