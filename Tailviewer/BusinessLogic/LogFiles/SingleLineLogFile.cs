using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using Metrolib;

namespace Tailviewer.BusinessLogic.LogFiles
{
	/// <summary>
	///     Responsible for separating multi-line log entries into single lines.
	/// </summary>
	/// <remarks>
	///     <see cref="LogFile" /> should probably be rewritten to no longer group log lines into multi-line entries
	///     and this class should be rewritten to merge multiple lines into log entries instead.
	///     But this way was easer to implement (sorry future me :P).
	/// </remarks>
	public sealed class SingleLineLogFile
		: AbstractLogFile
			, ILogFileListener
	{
		private const int BatchSize = 10000;
		private readonly TimeSpan _maximumWaitTime;

		private readonly ConcurrentQueue<LogFileSection> _pendingModifications;
		private readonly ILogFile _source;
		private LogFileSection _fullSourceSection;

		public SingleLineLogFile(ITaskScheduler scheduler, ILogFile source, TimeSpan maximumWaitTime)
			: base(scheduler)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));

			_pendingModifications = new ConcurrentQueue<LogFileSection>();

			_maximumWaitTime = maximumWaitTime;

			_source = source;
			_source.AddListener(this, maximumWaitTime, BatchSize);
			StartTask();
		}

		public override int Count
		{
			get { return _fullSourceSection.Count; }
		}

		public override int MaxCharactersPerLine
		{
			get { return _source.MaxCharactersPerLine; }
		}

		public override Size FileSize
		{
			get { return _source.FileSize; }
		}

		public override bool Exists
		{
			get { return _source.Exists; }
		}

		public override DateTime? StartTimestamp
		{
			get { return _source.StartTimestamp; }
		}

		public override DateTime LastModified
		{
			get { return _source.LastModified; }
		}

		public void OnLogFileModified(ILogFile logFile, LogFileSection section)
		{
			_pendingModifications.Enqueue(section);
			ResetEndOfSourceReached();
		}

		protected override void DisposeAdditional()
		{
			_source.RemoveListener(this);
		}

		public override void GetSection(LogFileSection section, LogLine[] dest)
		{
			_source.GetSection(section, dest);
			for (var i = 0; i < section.Count; ++i)
			{
				var line = Evaluate(dest[i]);
				dest[i] = new LogLine(line.LineIndex, line.LineIndex, line);
			}
		}

		public override LogLine GetLine(int index)
		{
			var line = Evaluate(_source.GetLine(index));
			return line;
		}

		[Pure]
		private static LogLine Evaluate(LogLine logLine)
		{
			var level = LogLine.DetermineLevelFromLine(logLine.Message);
			logLine = new LogLine(logLine.LineIndex, logLine.LineIndex, logLine.Message, level, logLine.Timestamp);
			return logLine;
		}

		private void Clear()
		{
			_fullSourceSection = new LogFileSection();
			Listeners.OnRead(-1);
		}

		protected override TimeSpan RunOnce(CancellationToken token)
		{
			LogFileSection section;
			while (_pendingModifications.TryDequeue(out section) && !token.IsCancellationRequested)
				if (section.IsReset)
				{
					Clear();
				}
				else if (section.InvalidateSection)
				{
					var startIndex = section.Index;
					_fullSourceSection = new LogFileSection(0, (int) startIndex);
					Listeners.Invalidate((int) startIndex, section.Count);
				}
				else
				{
					_fullSourceSection = LogFileSection.MinimumBoundingLine(_fullSourceSection, section);
				}

			Listeners.OnRead(_fullSourceSection.Count);
			SetEndOfSourceReached();

			return _maximumWaitTime;
		}
	}
}