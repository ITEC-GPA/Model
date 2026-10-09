param([string]$SourceBundle)
$ErrorActionPreference = 'Stop'
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
Push-Location $repository
try {
    & (Join-Path $PSScriptRoot 'Prepare-Dependencies.ps1') -SourceBundle $SourceBundle
    & (Join-Path $PSScriptRoot 'Test-PrepareDependencies.ps1')
    dotnet restore GPCModel.sln --locked-mode
    if ($LASTEXITCODE) { throw 'Locked restore failed.' }
    foreach ($project in @('Model.Core.Tests/Model.Core.Tests.csproj', 'UnitTest/UnitTest.csproj', 'ModelChecker.Tests/ModelChecker.Tests.csproj')) {
        dotnet test $project -c Release --no-restore -p:WarningLevel=0 --logger trx --results-directory TestResults/Architecture
        if ($LASTEXITCODE) { throw "Tests failed: $project" }
    }
    dotnet run --project tools/ApiSurface -c Release -- check ModelChecker.Tests/bin/Release/net6.0 build/api GPCModel GPCModelChecker GPCChecker.Concrete GPC.Model.Persistence
    if ($LASTEXITCODE) { throw 'Public API compatibility check failed.' }
} finally { Pop-Location }
