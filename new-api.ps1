param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^[A-Za-z][A-Za-z0-9_.-]*$')]
    [string]$ProjectName,

    [Parameter()]
    [string]$TemplateRoot = $PSScriptRoot,

    [Parameter()]
    [string]$OutputRoot = (Get-Location).Path,

    [Parameter()]
    [string]$TemplateSource,

    [Parameter()]
    [string]$TemplateIdentity = "BaseStructure.Template",

    [Parameter()]
    [string]$TemplateShortName = "basestructure",

    [Parameter()]
    [switch]$SkipInstall,

    [Parameter()]
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

function Invoke-Step {
    param(
        [string]$Title,
        [scriptblock]$Action
    )

    Write-Host "`n==> $Title" -ForegroundColor Cyan
    & $Action
}

function Assert-Command {
    param([string]$Name)

    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "Required command '$Name' was not found in PATH."
    }
}

Assert-Command -Name "dotnet"

if (-not $TemplateSource) {
    $TemplateSource = $TemplateRoot
}

if (-not (Test-Path $OutputRoot)) {
    throw "Output root '$OutputRoot' does not exist."
}

$projectRoot = Join-Path $OutputRoot $ProjectName
if (Test-Path $projectRoot) {
    throw "Target project folder already exists: '$projectRoot'."
}

if (-not $SkipInstall) {
    Invoke-Step -Title "Installing template from '$TemplateSource'" -Action {
        try {
            dotnet new uninstall $TemplateIdentity *> $null
        }
        catch {
            # Ignore missing-template uninstall errors on first run.
        }
        dotnet new install $TemplateSource --force
    }
}
else {
    Write-Host "Skipping template install as requested (-SkipInstall)." -ForegroundColor Yellow
}

Invoke-Step -Title "Creating project '$ProjectName' from template" -Action {
    Push-Location $OutputRoot
    try {
        dotnet new $TemplateShortName -n $ProjectName
    }
    finally {
        Pop-Location
    }
}

if (-not $SkipBuild) {
    $solutionPath = Join-Path $projectRoot "$ProjectName.sln"
    if (-not (Test-Path $solutionPath)) {
        throw "Generated solution not found: '$solutionPath'."
    }

    Invoke-Step -Title "Building generated solution" -Action {
        dotnet build $solutionPath
    }
}
else {
    Write-Host "Skipping build as requested (-SkipBuild)." -ForegroundColor Yellow
}

Write-Host "`nDone. Project created at: $projectRoot" -ForegroundColor Green
Write-Host "Next: dotnet run --project '$projectRoot\\$ProjectName\\$ProjectName.csproj'" -ForegroundColor Green

