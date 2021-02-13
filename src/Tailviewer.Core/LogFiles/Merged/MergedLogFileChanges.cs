using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles.Merged
{
	/// <summary>
	///     Responsible for keeping track of changes made to a <see cref="MergedLogFileIndex" /> in one
	///     <see cref="MergedLogFileIndex.Process(Tailviewer.Core.LogFiles.Merged.MergedLogFilePendingModification[])" />
	///     call.
	/// </summary>
	internal sealed class MergedLogFileChanges
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly List<LogFileSection> _changes;
		private readonly int _initialCount;
		private int _invalidationIndex;
		private int _count;

		public MergedLogFileChanges(int initialCount)
		{
			_initialCount = initialCount;
			_count = initialCount;
			_changes = new List<LogFileSection>();
			_invalidationIndex = -1;
		}

		public IReadOnlyList<LogFileSection> Sections => _changes;

		public bool TryGetFirstInvalidationIndex(out LogLineIndex index)
		{
			if (_invalidationIndex != -1)
			{
				index = _changes[_invalidationIndex].Index;
				return true;
			}

			index = LogLineIndex.Invalid;
			return false;
		}

		public void InvalidateFrom(LogLineIndex firstInvalidIndex)
		{
			if (firstInvalidIndex >= _count)
			{
				Log.WarnFormat("Ignoring invalidation from index '{0}' onwards because nothing has been appended there!", firstInvalidIndex);
				return;
			}

			if (firstInvalidIndex >= _initialCount)
			{
				// We do not need to add an invalidation, rather we can simply clamp an existing previous append
				if (firstInvalidIndex > 0)
				{
					var previous = _changes[_changes.Count - 1];
					if (!previous.IsInvalidate && !previous.IsReset)
					{
						var gap = previous.Index + previous.Count - firstInvalidIndex;
						if (gap > 0)
						{
							previous.Count -= gap;
							_changes[_changes.Count - 1] = previous;
						}
					}
				}
			}
			else
			{
				var invalidationCount = _count - firstInvalidIndex;
				if (_invalidationIndex != -1)
				{
					var invalidation = _changes[_invalidationIndex];
					if (invalidation.Index <= firstInvalidIndex)
						return; //< Nothing to do

					_changes[_invalidationIndex] = LogFileSection.Invalidate(firstInvalidIndex, invalidationCount);
				}
				else
				{
					_invalidationIndex = _changes.Count;
					_changes.Add(LogFileSection.Invalidate(firstInvalidIndex, invalidationCount));
				}
			}
		}

		public void Append(LogLineIndex index, int count)
		{
			if (count <= 0)
				return;

			if (TryGetLast(out var lastSection))
			{
				if (lastSection.IsInvalidate)
				{
					var gap = index - lastSection.Index;
					if (gap >= 0)
					{
						_changes.Add(new LogFileSection(lastSection.Index, count + gap));
					}
					else
					{
						throw new NotImplementedException();
					}
				}
				else if (lastSection.IsReset)
				{
					throw new NotImplementedException();
				}
				else
				{
					var gap = index - (lastSection.LastIndex + 1);
					if (gap > 0)
					{
						Log.WarnFormat("Inconsistency detected: Last change affects from '{0}' to '{1}' and the next one would leave a gap because it starts at '{2}'!",
							lastSection.Index, lastSection.LastIndex, index);
					}
					else if (gap < 0)
					{
						Log.WarnFormat("Inconsistency detected: Last change affects from '{0}' to '{1}' and the next one would overlap because it starts at '{2}'!",
							lastSection.Index, lastSection.LastIndex, index);
					}

					_changes[_changes.Count - 1] = new LogFileSection(lastSection.Index, lastSection.Count + gap + count);
				}
			}
			else
			{
				_changes.Add(new LogFileSection(index, count));
			}

			var last = _changes[_changes.Count - 1];
			_count = (int) (last.Index + last.Count);
		}

		private bool TryGetLast(out LogFileSection lastSection)
		{
			if (_changes.Count > 0)
			{
				lastSection = _changes[_changes.Count - 1];
				return true;
			}

			lastSection = new LogFileSection();
			return false;
		}

		public void Reset()
		{
			_changes.Clear();
			_changes.Add(LogFileSection.Reset);
			_invalidationIndex = -1;
		}
	}
}