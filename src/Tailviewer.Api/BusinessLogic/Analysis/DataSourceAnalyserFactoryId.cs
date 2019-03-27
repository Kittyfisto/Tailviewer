using System;
using Tailviewer.Ui.Analysis;

namespace Tailviewer.BusinessLogic.Analysis
{
	/// <summary>
	///     Identifies a <see cref="IDataSourceAnalyserPlugin"/> type (i.e. an implementation of the interface).
	/// </summary>
	public struct DataSourceAnalyserPluginId : IEquatable<DataSourceAnalyserPluginId>
	{
		/// <summary>
		///     This id may be used to specify that a <see cref="IWidgetPlugin" /> does not need a
		///     <see cref="IDataSourceAnalyserPlugin" />.
		/// </summary>
		public static readonly DataSourceAnalyserPluginId Empty = new DataSourceAnalyserPluginId();

		/// <inheritdoc />
		public bool Equals(DataSourceAnalyserPluginId other)
		{
			return string.Equals(Value, other.Value);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(objA: null, objB: obj)) return false;
			return obj is DataSourceAnalyserPluginId && Equals((DataSourceAnalyserPluginId)obj);
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
		public static bool operator ==(DataSourceAnalyserPluginId left, DataSourceAnalyserPluginId right)
		{
			return left.Equals(right);
		}

		/// <summary>
		///     Compares the two given ids for inequality.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator !=(DataSourceAnalyserPluginId left, DataSourceAnalyserPluginId right)
		{
			return !left.Equals(right);
		}

		private readonly string _value;

		/// <summary>
		///     Creates a new id from the given value.
		/// </summary>
		/// <param name="value"></param>
		public DataSourceAnalyserPluginId(string value)
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