using System;
using Tailviewer.Ui.Analysis;

namespace Tailviewer.BusinessLogic.Analysis
{
	/// <summary>
	///     Identifies either <see cref="ILogAnalyserPlugin"/> or a <see cref="IDataSourceAnalyserPlugin"/> type
	///     (i.e. an implementation of the interface).
	/// </summary>
	public struct AnalyserPluginId : IEquatable<AnalyserPluginId>
	{
		/// <summary>
		///     This id may be used to specify that a <see cref="IWidgetPlugin" /> does not need a
		///     <see cref="ILogAnalyserPlugin" />.
		/// </summary>
		public static readonly AnalyserPluginId Empty = new AnalyserPluginId();

		/// <inheritdoc />
		public bool Equals(AnalyserPluginId other)
		{
			return string.Equals(Value, other.Value);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(objA: null, objB: obj)) return false;
			return obj is AnalyserPluginId && Equals((AnalyserPluginId) obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		/// <summary>
		///     Compares the two given ids for equality.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator ==(AnalyserPluginId left, AnalyserPluginId right)
		{
			return left.Equals(right);
		}

		/// <summary>
		///     Compares the two given ids for inequality.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator !=(AnalyserPluginId left, AnalyserPluginId right)
		{
			return !left.Equals(right);
		}

		private readonly string _value;

		/// <summary>
		///     Creates a new id from the given value.
		/// </summary>
		/// <param name="value"></param>
		public AnalyserPluginId(string value)
		{
			_value = value;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return Value;
		}

		private string Value
		{
			get { return _value ?? string.Empty; }
		}
	}
}