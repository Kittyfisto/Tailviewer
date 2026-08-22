$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$binDir = Join-Path $repoRoot 'bin'
$stagingDir = Join-Path $repoRoot 'artifacts\portable'

if (Test-Path $stagingDir)
{
	Remove-Item $stagingDir -Recurse -Force
}
New-Item -ItemType Directory -Path $stagingDir -Force | Out-Null

function Copy-Required
{
	param(
		[string]$Source,
		[string]$RelativeDest
	)

	if (-not (Test-Path $Source))
	{
		throw "Required staging file missing: $Source"
	}

	$dest = Join-Path $stagingDir $RelativeDest
	$destDir = Split-Path $dest -Parent
	if (-not (Test-Path $destDir))
	{
		New-Item -ItemType Directory -Path $destDir -Force | Out-Null
	}

	Copy-Item $Source $dest -Force
	Write-Host "  + $RelativeDest"
}

Write-Host "Staging portable files to $stagingDir"

# Application + runtime assemblies. The rebrand (Tailviewer -> Tailviewer NEXT)
# ships "Tailviewer NEXT.exe" (an AfterBuild copy in Tailviewer.csproj); the
# original Tailviewer.exe stays as the reference assembly and is NOT distributed.
Copy-Required (Join-Path $binDir 'Tailviewer NEXT.exe') 'Tailviewer NEXT.exe'
Copy-Required (Join-Path $binDir 'Tailviewer NEXT.exe.config') 'Tailviewer NEXT.exe.config'
Copy-Required (Join-Path $binDir 'Tailviewer.Api.dll') 'Tailviewer.Api.dll'
Copy-Required (Join-Path $binDir 'Tailviewer.Core.dll') 'Tailviewer.Core.dll'
Copy-Required (Join-Path $binDir 'log4net.dll') 'log4net.dll'
Copy-Required (Join-Path $binDir 'Metrolib.dll') 'Metrolib.dll'
Copy-Required (Join-Path $binDir 'MMQ.dll') 'MMQ.dll'
Copy-Required (Join-Path $binDir 'SharpRemote.dll') 'SharpRemote.dll'
Copy-Required (Join-Path $binDir 'System.Extensions.dll') 'System.Extensions.dll'
Copy-Required (Join-Path $binDir 'Ookii.Dialogs.Wpf.dll') 'Ookii.Dialogs.Wpf.dll'
Copy-Required (Join-Path $binDir 'Xceed.Wpf.Toolkit.dll') 'Xceed.Wpf.Toolkit.dll'
Copy-Required (Join-Path $binDir 'archive.exe') 'archive.exe'

# Plugin package(s) produced by the Serilog plugin PostBuildEvent.
$pluginDir = Join-Path $binDir 'Plugins'
if (-not (Test-Path $pluginDir))
{
	throw "Plugin output missing: $pluginDir"
}
Get-ChildItem $pluginDir -Filter 'Tailviewer.Formats.Serilog.*.tvp' | ForEach-Object {
	Copy-Required $_.FullName ("Plugins\" + $_.Name)
}

# Installer helper files (kept alongside the Installer project).
Copy-Required (Join-Path $repoRoot 'src\Installer\InstallationFiles\Uninstall.cmd') 'Uninstall.cmd'
Copy-Required (Join-Path $repoRoot 'src\Installer\InstallationFiles\Plugins\Readme.txt') 'Plugins\Readme.txt'
Copy-Required (Join-Path $repoRoot 'LICENSE') 'LICENSE'

# Fonts + icon.
Copy-Required (Join-Path $repoRoot 'src\Tailviewer\Resources\Fonts\FONTLOG.txt') 'Fonts\FONTLOG.txt'
Copy-Required (Join-Path $repoRoot 'src\Tailviewer\Licenses\Inconsolata\OFL.txt') 'Fonts\OFL.txt'
Copy-Required (Join-Path $repoRoot 'src\Tailviewer\Resources\Fonts\METADATA.pb') 'Fonts\METADATA.pb'
Copy-Required (Join-Path $repoRoot 'src\Tailviewer\Resources\Tailviewer.ico') 'Icons\Tailviewer.ico'

# Localization satellites (app + AvalonDock) — copy whatever the build produced.
Get-ChildItem $binDir -Directory | ForEach-Object {
	$cultureDir = $_.FullName
	Get-ChildItem $cultureDir -Filter '*.resources.dll' -ErrorAction SilentlyContinue | ForEach-Object {
		$rel = $_.FullName.Substring($binDir.Length + 1)
		Copy-Required $_.FullName $rel
	}
}

Write-Host "Staging complete: $stagingDir"
