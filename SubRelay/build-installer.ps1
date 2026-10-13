param(
    [Parameter(Mandatory=$true)][string]$CompilerPath,
    [string]$DotnetPath = 'dotnet',
    [switch]$SkipPublish,
    [string]$PublishDirectory = (Join-Path $PSScriptRoot '..\output\SubRelay'),
    [string]$OutputDirectory = (Join-Path $PSScriptRoot '..\output\Installer')
)
$ErrorActionPreference = 'Stop'
if (-not $SkipPublish) { & (Join-Path $PSScriptRoot 'build.ps1') -DotnetPath $DotnetPath -OutputDirectory $PublishDirectory }
[xml]$project = Get-Content (Join-Path $PSScriptRoot 'SubRelay.csproj') -Raw
$version = [string]$project.Project.PropertyGroup.Version
$exe = Join-Path $PublishDirectory 'SubRelay.exe'
if ([Diagnostics.FileVersionInfo]::GetVersionInfo($exe).ProductVersion.Split('+')[0] -ne $version) { throw 'Published executable version does not match the project.' }
foreach ($file in @('README.md','THIRD-PARTY-NOTICES.txt','Licenses\DOTNET-LICENSE.txt')) {
    if (-not (Test-Path -LiteralPath (Join-Path $PublishDirectory $file))) { throw "Missing payload: $file" }
}
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
& $CompilerPath "/DAppVersion=$version" "/DPublishDir=$([IO.Path]::GetFullPath($PublishDirectory))" "/DInstallerOutputDir=$([IO.Path]::GetFullPath($OutputDirectory))" (Join-Path $PSScriptRoot 'Installer\SubRelay.iss')
if ($LASTEXITCODE -ne 0) { throw 'Installer build failed.' }
Write-Host "Built: $(Join-Path $OutputDirectory "SubRelay-Setup-$version.exe")"
