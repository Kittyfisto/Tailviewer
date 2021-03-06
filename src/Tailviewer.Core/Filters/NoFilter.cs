﻿using System.Collections.Generic;
using Tailviewer.Api;

// ReSharper disable once CheckNamespace
namespace Tailviewer.Core
{
	/// <summary>
	///     An <see cref="ILogLineFilter" /> implementation that passes every log line.
	/// </summary>
	internal sealed class NoFilter
		: ILogEntryFilter
	{
		/// <inheritdoc />
		public bool PassesFilter(IReadOnlyLogEntry logLine)
		{
			return true;
		}

		/// <inheritdoc />
		public List<LogLineMatch> Match(IReadOnlyLogEntry line)
		{
			return new List<LogLineMatch>();
		}

		/// <inheritdoc />
		public bool PassesFilter(IEnumerable<IReadOnlyLogEntry> logEntry)
		{
			return true;
		}

		/// <inheritdoc />
		public void Match(IReadOnlyLogEntry line, List<LogLineMatch> matches)
		{
			
		}
	}
}