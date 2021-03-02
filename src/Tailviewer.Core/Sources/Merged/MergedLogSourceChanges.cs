using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using Tailviewer.Api;

namespace Tailviewer.Core.Sources.Merged
{
	/// <summary>
	///     Responsible for keeping track of changes made to a <see cref="MergedLogSourceIndex" /> in one
	///     <see cref="MergedLogSourceIndex.Process(MergedLogSourcePendingModification[])" />
	///     call.
	/// </summary>
	/// <remarks>
	///    TODO: Rename to MergedLogSourceModifications
	/// </remarks>
	internal sealed class MergedLogSourceChanges
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly List<LogSourceModification> _modifications;
		private readonly int _initialCount;
		private int _removeIndex;
		private int _count;

		public MergedLogSourceChanges(int initialCount)
		{
			_initialCount = initialCount;
			_count = initialCount;
			_modifications = new List<LogSourceModification>();
			_removeIndex = -1;
		}

		public IReadOnlyList<LogSourceModification> Sections => _modifications;

		public bool TryGetFirstRemovedIndex(out LogLineIndex index)
		{
			if (_removeIndex != -1)
			{
				var modification = _modifications[_removeIndex];
				modification.IsRemoved(out var removedSection);
				index = removedSection.Index;
				return true;
			}

			index = LogLineIndex.Invalid;
			return false;
		}

		public void RemoveFrom(LogLineIndex firstInvalidIndex)
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
					var previous = _modifications[_modifications.Count - 1];
					if (previous.IsAppended(out var appendedSection))
					{
						var gap = appendedSection.Index + appendedSection.Count - firstInvalidIndex;
						if (gap > 0)
						{
							appendedSection.Count -= gap;
							_modifications[_modifications.Count - 1] = LogSourceModification.Appended(appendedSection);
						}
					}
				}
			}
			else
			{
				var invalidationCount = _count - firstInvalidIndex;
				if (_removeIndex != -1)
				{
					var invalidation = _modifications[_removeIndex];
					invalidation.IsRemoved(out var removedSection);
					if (removedSection.Index <= firstInvalidIndex)
						return; //< Nothing to do

					_modifications[_removeIndex] = LogSourceModification.Removed(firstInvalidIndex, invalidationCount);
				}
				else
				{
					_removeIndex = _modifications.Count;
					_modifications.Add(LogSourceModification.Removed(firstInvalidIndex, invalidationCount));
				}
			}
		}

		public void Append(LogLineIndex index, int count)
		{
			if (count <= 0)
				return;

			if (TryGetLast(out var lastSection))
			{
				if (lastSection.IsRemoved(out var removedSection))
				{
					var gap = index - removedSection.Index;
					if (gap >= 0)
					{
						_modifications.Add(LogSourceModification.Appended(removedSection.Index, count + gap));
					}
					else
					{
						throw new NotImplementedException();
					}
				}
				else if (lastSection.IsReset())
				{
					throw new NotImplementedException();
				}
				else if (lastSection.IsAppended(out var appendedSection))
				{
					var gap = index - (appendedSection.LastIndex + 1);
					if (gap > 0)
					{
						Log.WarnFormat("Inconsistency detected: Last change affects from '{0}' to '{1}' and the next one would leave a gap because it starts at '{2}'!",
						               appendedSection.Index, appendedSection.LastIndex, index);
					}
					else if (gap < 0)
					{
						Log.WarnFormat("Inconsistency detected: Last change affects from '{0}' to '{1}' and the next one would overlap because it starts at '{2}'!",
						               appendedSection.Index, appendedSection.LastIndex, index);
					}

					_modifications[_modifications.Count - 1] = LogSourceModification.Appended(appendedSection.Index, appendedSection.Count + gap + count);
				}
			}
			else
			{
				_modifications.Add(LogSourceModification.Appended(index, count));
			}

			var last = _modifications[_modifications.Count - 1];
			last.IsAppended(out var appendedSection2);
			_count = (int) (appendedSection2.Index + appendedSection2.Count);
		}

		private bool TryGetLast(out LogSourceModification lastModification)
		{
			if (_modifications.Count > 0)
			{
				lastModification = _modifications[_modifications.Count - 1];
				return true;
			}

			lastModification = default;
			return false;
		}

		public void Reset()
		{
			_modifications.Clear();
			_modifications.Add(LogSourceModification.Reset());
			_removeIndex = -1;
		}
	}
}