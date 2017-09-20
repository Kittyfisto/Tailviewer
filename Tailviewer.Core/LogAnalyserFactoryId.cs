using System;

namespace Tailviewer.Core
{
	/// <summary>
	///     Identifies a log analyer factory type (i.e. an implementation of the interface).
	/// </summary>
	/// <remarks>
	///     TODO: Make serializable
	/// </remarks>
	public struct LogAnalyserFactoryId : IEquatable<LogAnalyserFactoryId>
	{
		/// <summary>
		///     This id may be used to specify that a <see cref="IWidgetFactory" /> does not need a
		///     <see cref="ILogAnalyserFactory" />.
		/// </summary>
		public static readonly LogAnalyserFactoryId Empty = new LogAnalyserFactoryId();

		public bool Equals(LogAnalyserFactoryId other)
		{
			return string.Equals(_value, other._value);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(objA: null, objB: obj)) return false;
			return obj is LogAnalyserFactoryId && Equals((LogAnalyserFactoryId) obj);
		}

		public override int GetHashCode()
		{
			return _value != null ? _value.GetHashCode() : 0;
		}

		public static bool operator ==(LogAnalyserFactoryId left, LogAnalyserFactoryId right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(LogAnalyserFactoryId left, LogAnalyserFactoryId right)
		{
			return !left.Equals(right);
		}

		private readonly string _value;

		public LogAnalyserFactoryId(string value)
		{
			_value = value;
		}

		public override string ToString()
		{
			return _value ?? string.Empty;
		}
	}
}