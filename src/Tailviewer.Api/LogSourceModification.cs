using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Tailviewer
{
	/// <summary>
	///     Represents a modification of an <see cref="ILogSource" />.
	/// </summary>
	public readonly struct LogSourceModification : IEquatable<LogSourceModification>
	{
		private enum ModificationType
		{
			None,
			Appended,
			Removed,
			Reset,
			PropertiesChanged
		}

		private readonly ModificationType _modificationType;
		private readonly LogSourceSection _section;

		private LogSourceModification(ModificationType modificationType, LogSourceSection section)
		{
			_modificationType = modificationType;
			_section = section;
		}

		/// <summary>
		///     Tests if this modification represents an append operation to the original log source.
		/// </summary>
		/// <param name="appendedSection">The section of the log source which was appended.</param>
		/// <returns></returns>
		public bool IsAppended(out LogSourceSection appendedSection)
		{
			if (_modificationType == ModificationType.Appended)
			{
				appendedSection = _section;
				return true;
			}

			appendedSection = default;
			return false;
		}

		/// <summary>
		///     Tests if this modification represents a removed operation to the original log source.
		/// </summary>
		/// <param name="removedSection">The section of the log source which was removed.</param>
		/// <returns></returns>
		public bool IsRemoved(out LogSourceSection removedSection)
		{
			if (_modificationType == ModificationType.Removed)
			{
				removedSection = _section;
				return true;
			}

			removedSection = default;
			return false;
		}

		/// <summary>
		///     Tests if this modification represents a reset operation to the original log source.
		/// </summary>
		/// <returns></returns>
		public bool IsReset()
		{
			if (_modificationType == ModificationType.Reset) return true;

			return false;
		}

		/// <summary>
		///     Tests if this modification represents an append operation to the original log source.
		/// </summary>
		/// <returns></returns>
		public bool IsPropertiesChanged()
		{
			if (_modificationType == ModificationType.PropertiesChanged) return true;

			return false;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			switch (_modificationType)
			{
				case ModificationType.None:
					return "No Modification";
				case ModificationType.Appended:
					return $"Appended {_section}";
				case ModificationType.Removed:
					return $"Removed {_section}";
				case ModificationType.Reset:
					return "Reset";
				case ModificationType.PropertiesChanged:
					return "Properties Changed";
				default:
					return string.Format("Unknown ({0}}", _modificationType);
			}
		}

		#region Equality members

		/// <inheritdoc />
		public bool Equals(LogSourceModification other)
		{
			return _modificationType == other._modificationType && _section.Equals(other._section);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			return obj is LogSourceModification other && Equals(other);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				return ((int) _modificationType * 397) ^ _section.GetHashCode();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator ==(LogSourceModification left, LogSourceModification right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator !=(LogSourceModification left, LogSourceModification right)
		{
			return !left.Equals(right);
		}

		#endregion

		/// <summary>
		///     Creates a modification which signals to the listener that the given amount of <see cref="ILogEntry" />s were
		///     appended
		///     to the <see cref="ILogSource" />.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public static LogSourceModification Appended(LogLineIndex index, int count)
		{
			return Appended(new LogSourceSection(index, count));
		}

		/// <summary>
		///     Creates a modification which signals to the listener that the given amount of <see cref="ILogEntry" />s were
		///     appended
		///     to the <see cref="ILogSource" />.
		/// </summary>
		/// <param name="section"></param>
		/// <returns></returns>
		public static LogSourceModification Appended(LogSourceSection section)
		{
			return new LogSourceModification(ModificationType.Appended, section);
		}

		/// <summary>
		///     Creates a modification which signals to the listener that that all log entries beginning with
		///     <paramref name="index" />
		///     were removed from the <see cref="ILogSource" />.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		[Pure]
		public static LogSourceModification Removed(LogLineIndex index, int count)
		{
			return Removed(new LogSourceSection(index, count));
		}

		/// <summary>
		///     Creates a modification which signals to the listener that that all log entries beginning with
		///     <paramref name="section" />'s Index
		///     were removed from the <see cref="ILogSource" />.
		/// </summary>
		/// <param name="section"></param>
		/// <returns></returns>
		[Pure]
		public static LogSourceModification Removed(LogSourceSection section)
		{
			return new LogSourceModification(ModificationType.Removed, section);
		}

		/// <summary>
		///     Creates a modification which signals to the listener that that all log entries were removed from the
		///     <see cref="ILogSource" />.
		/// </summary>
		/// <returns></returns>
		[Pure]
		public static LogSourceModification Reset()
		{
			return new LogSourceModification(ModificationType.Reset, section: default);
		}

		/// <summary>
		///     Creates a modification which signals to the listener that something about a <see cref="ILogSource" />'s properties
		///     has changed.
		///     Properties may have been added / removed, values may have been changed, etc...
		/// </summary>
		/// <returns></returns>
		[Pure]
		public static LogSourceModification PropertiesChanged()
		{
			return new LogSourceModification(ModificationType.PropertiesChanged, section: default);
		}

		/// <summary>
		///     Splits up this section into multiple ones if it:
		///     - Is neither reset, nor invalidates
		///     - Appends more than the given amount of rows
		/// </summary>
		/// <param name="maxCount"></param>
		/// <returns></returns>
		public IEnumerable<LogSourceModification> Split(int maxCount)
		{
			if (maxCount <= 0)
				throw new ArgumentException("You need to specify a maximum count greater than 0!");

			if (_modificationType != ModificationType.Appended || _section.Count == 0) return new[] {this};

			return SplitAppend(maxCount);
		}

		private IEnumerable<LogSourceModification> SplitAppend(int maxCount)
		{
			var nextIndex = _section.Index;
			var remainingCount = _section.Count;
			while (remainingCount > maxCount)
			{
				yield return Appended(new LogSourceSection(nextIndex, maxCount));

				nextIndex += maxCount;
				remainingCount -= maxCount;
			}

			if (remainingCount > 0) yield return Appended(new LogSourceSection(nextIndex, remainingCount));
		}
	}
}