param([string]$Destination, [string]$SourceBundle)
$ErrorActionPreference = 'Stop'
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
if (!$Destination) { $Destination = Join-Path $repository '.dependencies' }
$Destination = [IO.Path]::GetFullPath($Destination)
[xml]$manifest = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'dependencies.props') -Raw
$resolved = foreach ($entry in $manifest.Project.ItemGroup.GpcDependency) {
    $cached = Join-Path $Destination $entry.Include
    # A verified pinned cache is independent of subsequent builds in sibling repositories.
    # An explicitly requested source bundle is always validated, even if a cache exists.
    $cachedIsValid = !$SourceBundle -and (Test-Path -LiteralPath $cached -PathType Leaf) -and
        (Get-FileHash -LiteralPath $cached -Algorithm SHA256).Hash -eq $entry.Sha256
    $source = if ($SourceBundle) { Join-Path $SourceBundle $entry.Include } elseif ($cachedIsValid) { $cached } else { Join-Path $repository $entry.Source }
    if (!(Test-Path -LiteralPath $source -PathType Leaf)) { throw "Missing pinned source: $source" }
    if ((Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash -ne $entry.Sha256) { throw "Source SHA256 mismatch: $source" }
    if ([Reflection.AssemblyName]::GetAssemblyName([IO.Path]::GetFullPath($source)).Version.ToString() -ne $entry.AssemblyVersion) { throw "Assembly version mismatch: $source" }
    [pscustomobject]@{ Source = $source; File = $entry.Include }
}
# Validate the complete source bundle before copying any file. No downloads or implicit upgrades.
New-Item -ItemType Directory -Path $Destination -Force | Out-Null
foreach ($entry in $resolved) {
    $target = Join-Path $Destination $entry.File
    if (![string]::Equals([IO.Path]::GetFullPath($entry.Source), [IO.Path]::GetFullPath($target), [StringComparison]::OrdinalIgnoreCase)) {
        Copy-Item -LiteralPath $entry.Source -Destination $target -Force
    }
}
Write-Host "Prepared $($resolved.Count) pinned dependencies at $Destination"
