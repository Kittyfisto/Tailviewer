param(
	[string]$OutputPath,

	[ValidateRange(1, 4096)]
	[int]$SizeMB = 514,

	[switch]$DecimalMB,

	[int]$Seed = 5142026,

	[switch]$Force
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$invariantCulture = [System.Globalization.CultureInfo]::InvariantCulture
$utf8 = [System.Text.UTF8Encoding]::new($false)
$chunkSize = 1MB

function Resolve-DefaultOutputPath
{
	param([string]$RequestedPath)

	if (-not [string]::IsNullOrWhiteSpace($RequestedPath))
	{
		return [System.IO.Path]::GetFullPath($RequestedPath)
	}

	$candidates = New-Object System.Collections.Generic.List[string]
	[void]$candidates.Add('D:\桌面')
	[void]$candidates.Add('D:\Desktop')

	$systemDesktop = [Environment]::GetFolderPath([Environment+SpecialFolder]::Desktop)
	if (-not [string]::IsNullOrWhiteSpace($systemDesktop))
	{
		[void]$candidates.Add($systemDesktop)
	}

	foreach ($candidate in $candidates)
	{
		if (Test-Path -LiteralPath $candidate -PathType Container)
		{
			return Join-Path $candidate 'Tailviewer-Demo-514MB.log'
		}
	}

	throw "Could not find a desktop directory. Checked: $($candidates -join ', ')"
}

function Flush-TextChunk
{
	param(
		[System.Text.StringBuilder]$Buffer,
		[System.IO.FileStream]$Stream,
		[System.Text.Encoding]$Encoding
	)

	if ($Buffer.Length -eq 0)
	{
		return [long]0
	}

	$text = $Buffer.ToString()
	$encoded = $Encoding.GetBytes($text)
	$Stream.Write($encoded, 0, $encoded.Length)
	[void]$Buffer.Clear()
	return [long]$encoded.Length
}

$targetPath = Resolve-DefaultOutputPath $OutputPath
$parentPath = [System.IO.Path]::GetDirectoryName($targetPath)

if (-not (Test-Path -LiteralPath $parentPath -PathType Container))
{
	throw "Output directory does not exist: $parentPath"
}

if ((Test-Path -LiteralPath $targetPath -PathType Leaf) -and -not $Force)
{
	throw "Output file already exists. Use -Force to replace it: $targetPath"
}

[long]$targetBytes = if ($DecimalMB)
{
	[long]$SizeMB * 1000000
}
else
{
	[long]$SizeMB * 1MB
}

$temporaryName = '.{0}.{1}.tmp' -f [System.IO.Path]::GetFileName($targetPath), [Guid]::NewGuid().ToString('N')
$temporaryPath = Join-Path $parentPath $temporaryName
$random = [System.Random]::new($Seed)
$buffer = [System.Text.StringBuilder]::new($chunkSize)
$stream = $null
$completed = $false
[long]$bytesWritten = 0
[long]$lineNumber = 0
[long]$nextProgress = 50MB
[int]$lastProgressPercent = -1

$processIds = @(4820, 5176, 6312, 7448, 8092, 9124)
$components = @(
	'Tailviewer.Demo.Api.RequestPipeline',
	'Tailviewer.Demo.Core.LogIngestionWorker',
	'Tailviewer.Demo.Core.CheckpointStore',
	'Tailviewer.Demo.Infrastructure.CacheProvider',
	'Tailviewer.Demo.Infrastructure.SqlConnectionPool',
	'Tailviewer.Demo.Hosting.HealthMonitor'
)
$routes = @(
	'GET /api/v1/streams',
	'GET /api/v1/streams/demo/events',
	'POST /api/v1/query',
	'PUT /api/v1/checkpoints',
	'GET /health/ready',
	'POST /api/v1/export'
)
$dependencies = @('log-index', 'metrics-store', 'config-service', 'archive-writer', 'search-cluster')
$eventNames = @('LogBatchReceived', 'CheckpointAdvanced', 'FilterApplied', 'IndexSegmentMerged', 'ExportProgressed', 'ConnectionRenewed')
$exceptionTypes = @('TimeoutException', 'IOException', 'SocketException', 'InvalidOperationException')
$fileNames = @('application.log', 'worker-03.log', 'audit-2026-08-23.log', 'events.ndjson', 'tailviewer-session.log')
$initialLevels = @('INFO', 'DEBUG', 'WARN', 'ERROR')
$startTime = [datetime]::ParseExact('2026-08-23 09:00:00', 'yyyy-MM-dd HH:mm:ss', $invariantCulture)

Write-Host ('Generating {0:N0} bytes ({1} {2}) at {3}' -f $targetBytes, $SizeMB, $(if ($DecimalMB) { 'decimal MB' } else { 'MiB' }), $targetPath)

try
{
	$fileMode = [System.IO.FileMode]::CreateNew
	$stream = [System.IO.FileStream]::new(
		$temporaryPath,
		$fileMode,
		[System.IO.FileAccess]::Write,
		[System.IO.FileShare]::Read,
		$chunkSize,
		[System.IO.FileOptions]::SequentialScan)

	# Keep enough space for one final, exact-size line. All generated content is
	# ASCII, which is also valid UTF-8, so character count equals byte count.
	$finalLineReserve = 1024
	while (($bytesWritten + $buffer.Length + $finalLineReserve) -lt $targetBytes)
	{
		$slot = [int]($lineNumber % 100)
		if ($lineNumber -lt $initialLevels.Count)
		{
			$level = $initialLevels[[int]$lineNumber]
		}
		elseif ($slot -lt 68)
		{
			$level = 'INFO'
		}
		elseif ($slot -lt 86)
		{
			$level = 'DEBUG'
		}
		elseif ($slot -lt 97)
		{
			$level = 'WARN'
		}
		else
		{
			$level = 'ERROR'
		}

		$processId = $processIds[$random.Next(0, $processIds.Count)]
		$threadId = $random.Next(1, 48)
		$component = $components[$random.Next(0, $components.Count)]
		$route = $routes[$random.Next(0, $routes.Count)]
		$dependency = $dependencies[$random.Next(0, $dependencies.Count)]
		$eventName = $eventNames[$random.Next(0, $eventNames.Count)]
		$requestId = 'req-{0:x8}' -f $random.Next(0, [int]::MaxValue)
		$spanId = 'span-{0:x6}' -f $random.Next(0, 16777215)
		$timestamp = $startTime.AddMilliseconds([double](($lineNumber * 125) + $random.Next(0, 80)))
		$timestampText = $timestamp.ToString('yyyy-MM-dd HH:mm:ss,fff', $invariantCulture)

		$durationMs = $random.Next(1, 2500)
		$bytesValue = $random.Next(512, 9000000)
		$queueDepth = $random.Next(0, 1800)
		$partition = $random.Next(0, 24)
		$offset = $random.Next(10000, 99000000)
		$statusCode = if ($level -eq 'ERROR') { $random.Next(500, 504) } elseif ($level -eq 'WARN') { 429 } else { 200 }
		$cpu = ($random.NextDouble() * 100).ToString('F1', $invariantCulture)
		$ratio = ($random.NextDouble()).ToString('F4', $invariantCulture)
		$latencyMs = $random.Next(1, 800)
		$retry = $random.Next(0, 4)
		$fileName = $fileNames[$random.Next(0, $fileNames.Count)]
		$exceptionType = $exceptionTypes[$random.Next(0, $exceptionTypes.Count)]

		$messageKind = $random.Next(0, 4)
		switch ($level)
		{
			'INFO'
			{
				switch ($messageKind)
				{
					0 { $message = "Request completed route=$route status=$statusCode durationMs=$durationMs bytes=$bytesValue" }
					1 { $message = "Batch accepted event=$eventName partition=$partition offset=$offset records=$($random.Next(10, 6000))" }
					2 { $message = "Dependency healthy name=$dependency latencyMs=$latencyMs nextCheckSeconds=$($random.Next(5, 60))" }
					3 { $message = "File scan completed path=C:\ProgramData\Tailviewer\Data\$fileName lines=$($random.Next(100, 50000)) checksum=$('{0:x8}' -f $random.Next(0, [int]::MaxValue))" }
				}
			}
			'DEBUG'
			{
				switch ($messageKind)
				{
					0 { $message = "Dispatching event name=$eventName requestId=$requestId queueDepth=$queueDepth retry=$retry" }
					1 { $message = "Cache lookup key=stream:${partition}:${offset} hit=$([bool]($random.Next(0, 2))) ttlSeconds=$($random.Next(30, 3600))" }
					2 { $message = "Reading chunk path=C:\ProgramData\Tailviewer\Data\$fileName offset=$offset length=$bytesValue" }
					3 { $message = "Connection pool state dependency=$dependency active=$($random.Next(1, 32)) idle=$($random.Next(0, 16)) waiters=$($random.Next(0, 12))" }
				}
			}
			'WARN'
			{
				switch ($messageKind)
				{
					0 { $message = "Retry scheduled route=$route attempt=$retry backoffMs=$($random.Next(100, 5000)) reason=transient-failure" }
					1 { $message = "Queue depth above threshold queue=ingestion depth=$queueDepth threshold=1200 consumerLagMs=$($random.Next(500, 9000))" }
					2 { $message = "Slow request route=$route durationMs=$durationMs thresholdMs=1000 requestId=$requestId" }
					3 { $message = "Configuration key not found key=features.experimentalFilters source=config-service fallback=default" }
				}
			}
			'ERROR'
			{
				switch ($messageKind)
				{
					0 { $message = "Request failed route=$route status=$statusCode exception=$exceptionType durationMs=$durationMs requestId=$requestId" }
					1 { $message = "Failed to persist checkpoint partition=$partition offset=$offset exception=$exceptionType retryable=true" }
					2 { $message = "Unhandled processing error event=$eventName exception=$exceptionType inner=connection-reset spanId=$spanId" }
					3 { $message = "Export job aborted file=$fileName bytesWritten=$bytesValue exception=$exceptionType correlation=$requestId" }
				}
			}
		}

		$logLine = '{0} [{1}, {2}] {3,-5} {4} ({5}) - {6} cpuPct={7} sampleRatio={8}' -f `
			$timestampText, $processId, $threadId, $level, $component, $spanId, $message, $cpu, $ratio
		$lineBytes = $logLine.Length + 2
		if (($bytesWritten + $buffer.Length + $lineBytes + $finalLineReserve) -gt $targetBytes)
		{
			break
		}

		[void]$buffer.Append($logLine).Append("`r`n")
		$lineNumber++

		if ($buffer.Length -ge $chunkSize)
		{
			$bytesWritten += Flush-TextChunk $buffer $stream $utf8
			if ($bytesWritten -ge $nextProgress)
			{
				$progressPercent = [int][math]::Floor(($bytesWritten * 100) / $targetBytes)
				if ($progressPercent -gt $lastProgressPercent)
				{
					Write-Progress -Activity 'Generating synthetic log' -Status ("{0:N0} / {1:N0} bytes" -f $bytesWritten, $targetBytes) -PercentComplete $progressPercent
					$lastProgressPercent = $progressPercent
				}
				$nextProgress += 50MB
			}
		}
	}

	$bytesWritten += Flush-TextChunk $buffer $stream $utf8
	$remaining = $targetBytes - $bytesWritten
	$finalTimestamp = $startTime.AddMilliseconds([double](($lineNumber * 125) + 79)).ToString('yyyy-MM-dd HH:mm:ss,fff', $invariantCulture)
	$finalPrefix = '{0} [4820, 1] INFO  Tailviewer.Demo.Hosting.SyntheticLogWriter (final) - Synthetic stream completed finalChunk=true filler=' -f $finalTimestamp
	$finalPrefixBytes = $utf8.GetByteCount($finalPrefix)
	if ($remaining -lt ($finalPrefixBytes + 2))
	{
		throw "Could not fit the exact-size final line; remaining bytes: $remaining"
	}

	$fillerLength = [int]($remaining - $finalPrefixBytes - 2)
	$filler = [string]::new([char]'x', $fillerLength)
	$finalLine = $finalPrefix + $filler + "`r`n"
	$finalBytes = $utf8.GetBytes($finalLine)
	if ($finalBytes.Length -ne $remaining)
	{
		throw "Final line size mismatch: expected $remaining bytes, got $($finalBytes.Length)"
	}
	$stream.Write($finalBytes, 0, $finalBytes.Length)
	$bytesWritten += $finalBytes.Length
	$stream.Flush()
	$stream.Dispose()
	$stream = $null

	$temporaryLength = (Get-Item -LiteralPath $temporaryPath).Length
	if ($temporaryLength -ne $targetBytes)
	{
		throw "Temporary file size mismatch: expected $targetBytes bytes, got $temporaryLength"
	}

	if (Test-Path -LiteralPath $targetPath -PathType Leaf)
	{
		if (-not $Force)
		{
			throw "Output file appeared while generating and -Force was not specified: $targetPath"
		}
		[System.IO.File]::Delete($targetPath)
	}
	[System.IO.File]::Move($temporaryPath, $targetPath)
	$completed = $true
	Write-Progress -Activity 'Generating synthetic log' -Completed
}
finally
{
	if ($null -ne $stream)
	{
		$stream.Dispose()
	}
	if (-not $completed -and (Test-Path -LiteralPath $temporaryPath -PathType Leaf))
	{
		try
		{
			Remove-Item -LiteralPath $temporaryPath -Force -ErrorAction SilentlyContinue
		}
		catch
		{
			Write-Warning "Could not remove incomplete temporary file: $temporaryPath"
		}
	}
}

$result = Get-Item -LiteralPath $targetPath
Write-Host ('Created {0} ({1:N0} bytes, {2:N2} MiB) with {3:N0} complete log lines.' -f $result.FullName, $result.Length, ($result.Length / 1MB), ($lineNumber + 1))
