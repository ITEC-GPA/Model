param([string]$SourceBundle, [string]$Results)
# -Results: folder for the test results (<project>.trx); default TestResults/Architecture. Checker/build/Release-Set.ps1 passes
# a fresh folder and reads the counts from it.
$ErrorActionPreference = 'Stop'
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
if (!$Results) { $Results = Join-Path $repository 'TestResults/Architecture' }
Push-Location $repository
try {
    & (Join-Path $PSScriptRoot 'Prepare-Dependencies.ps1') -SourceBundle $SourceBundle
    & (Join-Path $PSScriptRoot 'Test-PrepareDependencies.ps1')
    dotnet restore GPCModel.sln --locked-mode
    if ($LASTEXITCODE) { throw 'Locked restore failed.' }
    foreach ($project in @('Model.Core.Tests/Model.Core.Tests.csproj', 'UnitTest/UnitTest.csproj')) {
        $name = [IO.Path]::GetFileNameWithoutExtension($project)
        dotnet test $project -c Release --no-restore -p:WarningLevel=0 --logger "trx;LogFileName=$name.trx" --results-directory $Results
        if ($LASTEXITCODE) { throw "Tests failed: $project" }
    }
    # GPCModelChecker, its tests and its API baseline live in the Checker repository (Checker/build/Verify.ps1).
    dotnet run --project tools/ApiSurface -c Release -- check UnitTest/bin/Release/net6.0 build/api GPCModel GPC.Model.Persistence
    if ($LASTEXITCODE) { throw 'Public API compatibility check failed.' }
} finally { Pop-Location }
