param(
    [string]$GameRoot = (Join-Path $PSScriptRoot '..\..')
)

$ErrorActionPreference = 'Stop'
$gamePath = (Resolve-Path -LiteralPath $GameRoot).Path.TrimEnd('\', '/')
if (!(Test-Path -LiteralPath (Join-Path $gamePath 'BepInEx\core')) -or !(Test-Path -LiteralPath (Join-Path $gamePath 'KingdomInc_Data\Managed'))) {
    throw "Invalid Kingdom Inc GameRoot '$gamePath': expected BepInEx\\core and KingdomInc_Data\\Managed."
}
$project = Join-Path $PSScriptRoot 'PortLayouts.csproj'

& dotnet build $project -c Release "-p:GameRoot=$($gamePath.Replace('\', '/') + '/')"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "Built: $(Join-Path $PSScriptRoot 'bin\Release\netstandard2.1\PortLayouts.dll')"
