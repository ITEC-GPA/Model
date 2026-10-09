$ErrorActionPreference = 'Stop'
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$prepare = Join-Path $PSScriptRoot 'Prepare-Dependencies.ps1'
$bundle = Join-Path $repository '.dependencies'
$run = Join-Path $repository ('TestResults/DependencyPreparation/' + [Guid]::NewGuid().ToString('N'))
$destination = Join-Path $run 'consumer'
& $prepare -SourceBundle $bundle -Destination $destination
# The verified cache must work without relying on sibling build outputs.
& $prepare -Destination $destination
# Reusing the same validated directory must not attempt to copy a file over itself.
& $prepare -SourceBundle $destination -Destination $destination
$invalid = Join-Path $run 'invalid-source'
New-Item -ItemType Directory -Path $invalid -Force | Out-Null
Set-Content -LiteralPath (Join-Path $invalid 'DelaunayMesh.dll') -Value 'invalid bundle'
$before = (Get-FileHash -LiteralPath (Join-Path $destination 'DelaunayMesh.dll')).Hash
$rejected = $false
try { & $prepare -SourceBundle $invalid -Destination $destination }
catch {
    if ($_.Exception.Message -notlike 'Source SHA256 mismatch:*') { throw }
    $rejected = $true
}
if (!$rejected) { throw 'An explicit invalid source must not be hidden by the cache.' }
if ($before -ne (Get-FileHash -LiteralPath (Join-Path $destination 'DelaunayMesh.dll')).Hash) { throw 'A rejected source modified the destination.' }
Write-Host 'Dependency preparation: fresh bundle, cached bundle, same-directory reuse and invalid-source rejection PASS.'
