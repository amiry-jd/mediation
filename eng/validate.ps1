param(
    [switch]$SkipBenchmarks
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$artifacts = Join-Path $repoRoot 'artifacts'
$coverage = Join-Path $artifacts 'coverage'
$testResults = Join-Path $artifacts 'test-results'
$packages = Join-Path $artifacts 'packages'
$coverageFile = Join-Path $coverage 'coverage.cobertura.xml'

New-Item -ItemType Directory -Force -Path $artifacts, $coverage, $testResults, $packages | Out-Null

dotnet restore "$repoRoot/Mediation.slnx" --locked-mode
dotnet build "$repoRoot/Mediation.slnx" --configuration Release --no-restore
dotnet test "$repoRoot/tests/Mediation.Tests/Mediation.Tests.csproj" --configuration Release --no-build --logger "trx;LogFileName=tests.trx" --results-directory $testResults /p:CollectCoverage=true /p:CoverletOutput="$coverage/" /p:CoverletOutputFormat=cobertura /p:Threshold=90 /p:ThresholdType=line /p:ThresholdStat=total
dotnet tool restore
dotnet reportgenerator -reports:$coverageFile -targetdir:"$coverage/report" -reporttypes:"HtmlInline_AzurePipelines;Cobertura;MarkdownSummaryGithub"
dotnet run --configuration Release --project "$repoRoot/tools/ApiCompat/ApiCompat.csproj" --no-build -- "$repoRoot"
dotnet list "$repoRoot/Mediation.slnx" package --vulnerable --include-transitive
dotnet pack "$repoRoot/Mediation/Mediation.csproj" --configuration Release --no-build --output $packages

if (-not $SkipBenchmarks) {
    dotnet run --configuration Release --project "$repoRoot/benchmarks/Mediation.Benchmarks/Mediation.Benchmarks.csproj" -- --filter '*' --job short --artifacts "$artifacts/benchmarks"
}
