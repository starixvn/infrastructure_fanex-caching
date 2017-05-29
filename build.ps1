[CmdletBinding(PositionalBinding=$false)]
param(
    [string] $BuildNumber,
    [bool] $CreatePackages,
    [bool] $BuildForAspNetCore = $false,
    [bool] $RunTests = $true
)

Write-Host "Run Parameters:" -ForegroundColor Cyan
Write-Host "BuildNumber: $BuildNumber"
Write-Host "CreatePackages: $CreatePackages"
Write-Host "RunTests: $RunTests"

$packageOutputFolder = "$PSScriptRoot\.nupkgs"
$projectsToBuild =
    'Fanex.Caching',
    'Fanex.Caching.Redis'

$testsToRun =
    'Fanex.Caching.Tests',
    'Fanex.Caching.Redis.Tests'

if (!$BuildNumber) {
    Write-Host "ERROR: You must supply -BuildNumber argument. `
    Use -BuildNumber `"12345`" for generation using <configured version>-<buildnumber>" -ForegroundColor Yellow
    Exit 1
}

if ($RunTests) {
    dotnet restore /ConsoleLoggerParameters:Verbosity=Quiet
    foreach ($project in $testsToRun) {
        Write-Host "Running tests: $project (all frameworks)" -ForegroundColor "Magenta"
        Push-Location ".\Tests\$project"

        dotnet xunit
        if ($LastExitCode -ne 0) {
            Write-Host "Error with tests, aborting build." -Foreground "Red"
            Pop-Location
            Exit 1
        }

        Write-Host "Tests passed!" -ForegroundColor "Green"
        Pop-Location
    }
}

if ($CreatePackages) {
    mkdir -Force $packageOutputFolder | Out-Null
    Write-Host "Clearing existing $packageOutputFolder..." -NoNewline
    Get-ChildItem $packageOutputFolder | Remove-Item
    Write-Host "Done." -ForegroundColor "Green"

    Write-Host "Building all packages" -ForegroundColor "Green"

    foreach ($project in $projectsToBuild) {
        Write-Host "Working on $project`:" -ForegroundColor "Magenta"
        Write-Host "  Restoring and packing $project... (BuildNumber:" -NoNewline -ForegroundColor "Magenta"
        Write-Host $BuildNumber -NoNewline -ForegroundColor "Cyan"
        Write-Host ")" -ForegroundColor "Magenta"

        Push-Location ".\Sources\$project"

        $targets = "Restore;Pack"

        if ($BuildForAspNetCore) {
            dotnet msbuild `
                "/t:$targets" `
                "/p:Configuration=Release" `
                "/p:DefineConstants=COREFX" `
                "/p:VersionSufix=$BuildNumber" `
                "/p:PackageOutputPath=$packageOutputFolder" `
                "/p:NuGetBuildTasksPackTargets='000'"
        } else {
            dotnet msbuild `
                "/t:$targets" `
                "/p:Configuration=Release" `
                "/p:VersionSufix=$BuildNumber" `
                "/p:PackageOutputPath=$packageOutputFolder" `
                "/p:NuGetBuildTasksPackTargets='000'"
        }

        Pop-Location

        Write-Host "Done." -ForegroundColor "Green"
        Write-Host ""
    }

    Write-Host "Build Complete." -ForegroundColor "Green"
}