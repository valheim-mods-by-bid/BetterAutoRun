[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$ProjectDirectory,

    [Parameter(Mandatory = $true)]
    [string]$BinaryPath,

    [Parameter(Mandatory = $true)]
    [string]$ArtifactsDirectory
)

$ErrorActionPreference = 'Stop'

$projectRoot = [IO.Path]::GetFullPath($ProjectDirectory)
$binary = [IO.Path]::GetFullPath($BinaryPath)
$artifacts = [IO.Path]::GetFullPath($ArtifactsDirectory)
$repositoryRoot = $projectRoot
$packageSource = Join-Path $projectRoot 'Package'

if (-not (Test-Path -LiteralPath $binary -PathType Leaf)) {
    throw "Plugin binary not found: $binary"
}

$assemblyVersion = [Reflection.AssemblyName]::GetAssemblyName($binary).Version
$version = '{0}.{1}.{2}' -f $assemblyVersion.Major, $assemblyVersion.Minor, $assemblyVersion.Build
$stagingRoot = Join-Path $artifacts '.package'
$stagingDirectory = Join-Path $stagingRoot 'BetterAutoRun'
$pluginDirectory = Join-Path $stagingDirectory 'plugins'
$archivePath = Join-Path $artifacts "BetterAutoRun-$version.zip"

if (Test-Path -LiteralPath $stagingRoot) {
    $resolvedStaging = [IO.Path]::GetFullPath($stagingRoot)
    if (-not $resolvedStaging.StartsWith($artifacts, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to clean staging directory outside artifacts: $resolvedStaging"
    }
    Remove-Item -LiteralPath $resolvedStaging -Recurse -Force
}

New-Item -ItemType Directory -Path $pluginDirectory -Force | Out-Null
Copy-Item -LiteralPath $binary -Destination (Join-Path $pluginDirectory 'BetterAutoRun.dll')
Copy-Item -LiteralPath (Join-Path $packageSource 'icon.png') -Destination (Join-Path $stagingDirectory 'icon.png')
Copy-Item -LiteralPath (Join-Path $packageSource 'README.md') -Destination (Join-Path $stagingDirectory 'README.md')
Copy-Item -LiteralPath (Join-Path $repositoryRoot 'CHANGELOG.md') -Destination (Join-Path $stagingDirectory 'CHANGELOG.md')
Copy-Item -LiteralPath (Join-Path $repositoryRoot 'LICENSE') -Destination (Join-Path $stagingDirectory 'LICENSE')
Copy-Item -LiteralPath (Join-Path $repositoryRoot 'NOTICE.md') -Destination (Join-Path $stagingDirectory 'NOTICE.md')

$manifestPath = Join-Path $packageSource 'manifest.json'
$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
$manifest.version_number = $version
$manifest | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $stagingDirectory 'manifest.json') -Encoding UTF8

if (Test-Path -LiteralPath $archivePath) {
    Remove-Item -LiteralPath $archivePath -Force
}

Compress-Archive -Path (Join-Path $stagingDirectory '*') -DestinationPath $archivePath -CompressionLevel Optimal
Remove-Item -LiteralPath $stagingRoot -Recurse -Force

Write-Host "Created package: $archivePath"
