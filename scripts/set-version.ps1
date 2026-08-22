param(
	[Parameter(Mandatory = $true)]
	[string]$Version
)

$ErrorActionPreference = 'Stop'

if ($Version -notmatch '^\d+\.\d+\.\d+$')
{
	throw "Version must be in x.y.z format, got '$Version'"
}

$assemblyVersion = "$Version.0"

$infoPath = Join-Path $PSScriptRoot '..\src\GlobalAssemblyInfo.cs'
if (-not (Test-Path $infoPath))
{
	throw "GlobalAssemblyInfo.cs not found: $infoPath"
}

# GlobalAssemblyInfo.cs is Windows-1252 (no BOM). Read/write in that encoding to
# preserve the non-ASCII characters in the copyright line.
$encoding = [System.Text.Encoding]::GetEncoding(1252)
$content = [System.IO.File]::ReadAllText($infoPath, $encoding)

$content = [regex]::Replace($content, '(?m)^\[assembly:\s*AssemblyVersion\("[^"]*"\)\]', "[assembly: AssemblyVersion(`"$assemblyVersion`")]")
$content = [regex]::Replace($content, '(?m)^\[assembly:\s*AssemblyFileVersion\("[^"]*"\)\]', "[assembly: AssemblyFileVersion(`"$Version`")]")
$content = [regex]::Replace($content, '(?m)^\[assembly:\s*AssemblyInformationalVersion\("[^"]*"\)\]', "[assembly: AssemblyInformationalVersion(`"$Version`")]")

[System.IO.File]::WriteAllText($infoPath, $content, $encoding)

Write-Host "Version injected into src/GlobalAssemblyInfo.cs:"
Write-Host "  AssemblyVersion              = $assemblyVersion"
Write-Host "  AssemblyFileVersion          = $Version"
Write-Host "  AssemblyInformationalVersion = $Version"
