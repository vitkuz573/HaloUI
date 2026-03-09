Param(
    [string]$Configuration = "Release",
    [string]$SnapshotOutput = "artifacts/themesdk/current/ThemeSdkSnapshot.json",
    [string]$ChangeReport = "artifacts/themesdk/changes/ThemeSdkChanges.txt",
    [switch]$UpdateBaseline,
    [switch]$Json,
    [switch]$UpdateSummary
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $repoRoot

function Resolve-PathRelative([string]$relative)
{
    if ([System.IO.Path]::IsPathRooted($relative)) { return $relative }
    return [System.IO.Path]::Combine($repoRoot, $relative)
}

$snapshotPath = Resolve-PathRelative $SnapshotOutput
$changeReportPath = Resolve-PathRelative $ChangeReport
$changeReportDirectory = Split-Path -Parent $changeReportPath
$baselinePath = Resolve-PathRelative "artifacts/themesdk/baseline/ThemeSdkBaseline.json"
$summaryPath = Resolve-PathRelative "artifacts/themesdk/changes/ChangesSinceLastRelease.txt"
$baselineDirectory = Split-Path -Parent $baselinePath

New-Item -ItemType Directory -Path $changeReportDirectory -Force | Out-Null
New-Item -ItemType Directory -Path $baselineDirectory -Force | Out-Null

Write-Host "Generating Theme SDK snapshot..."
dotnet run --project (Resolve-PathRelative "tools/ThemeSdk.SnapshotTool/ThemeSdk.SnapshotTool.csproj") --configuration $Configuration -- --output $snapshotPath

$format = if ($Json) { "json" } else { "text" }

if (-not (Test-Path $baselinePath))
{
    if ($UpdateBaseline)
    {
        Write-Host "Baseline not found. Creating baseline from current snapshot..."
        Copy-Item $snapshotPath $baselinePath -Force
    }
    else
    {
        throw "Theme SDK baseline was not found at '$baselinePath'. Run once with -UpdateBaseline to initialize it."
    }
}

Write-Host "Diffing snapshot against baseline..."
dotnet run --project (Resolve-PathRelative "tools/ThemeSdk.DiffTool/ThemeSdk.DiffTool.csproj") --configuration $Configuration -- --old $baselinePath --new $snapshotPath --output $changeReportPath --format $format

if ($UpdateSummary -and -not $Json)
{
    Write-Host "Appending summary at $summaryPath"
    $logContent = Get-Content $changeReportPath -Raw
    if (-not [string]::IsNullOrWhiteSpace($logContent))
    {
        if (Test-Path $summaryPath)
        {
            $existingLength = (Get-Item $summaryPath).Length
            if ($existingLength -gt 0)
            {
                Add-Content -Path $summaryPath -Value ""
            }
        }
        else
        {
            New-Item -ItemType File -Path $summaryPath -Force | Out-Null
        }

        Add-Content -Path $summaryPath -Value $logContent
    }
}

if ($UpdateBaseline)
{
    Write-Host "Updating baseline with current snapshot..."
    Copy-Item $snapshotPath $baselinePath -Force
}

Write-Host "Theme SDK change report written to $changeReportPath"
