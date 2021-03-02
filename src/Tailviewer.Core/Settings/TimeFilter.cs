using System;
using System.Diagnostics.Contracts;
using System.Xml;
using Metrolib;
using Tailviewer.Api;
using Tailviewer.Core.Filters;
using Tailviewer.Core.Filters.ExpressionEngine;

namespace Tailviewer.Core.Settings
{
	/// <summary>
	///     The configuration of a filter which allows filtering
	///     log events by their timestamp.
	/// </summary>
	public sealed class TimeFilter
		: ICloneable
	{
		/// <summary>
		/// 
		/// </summary>
		public TimeFilterMode Mode;

		/// <summary>
		///     One of the pre-defined special ranges (such as today).
		/// </summary>
		public SpecialDateTimeInterval SpecialInterval;

		/// <summary>
		/// </summary>
		public DateTime? Maximum;

		/// <summary>
		/// </summary>
		public DateTime? Minimum;

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[Pure]
		public TimeFilter Clone()
		{
			return new TimeFilter
			{
				Mode = Mode,
				SpecialInterval = SpecialInterval,
				Minimum = Minimum,
				Maximum = Maximum
			};
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		/// <summary>
		///     Restores this filter from the given xml reader.
		/// </summary>
		/// <param name="reader"></param>
		/// <returns></returns>
		public bool Restore(XmlReader reader)
		{
			var count = reader.AttributeCount;
			for (var i = 0; i < count; ++i)
			{
				reader.MoveToAttribute(i);

				switch (reader.Name)
				{
					case "mode":
						if (Enum.TryParse(reader.Value, ignoreCase: true, result: out TimeFilterMode mode))
							Mode = mode;
						break;

					case "range":
						if (Enum.TryParse(reader.Value, ignoreCase: true, result: out SpecialDateTimeInterval range))
							SpecialInterval = range;
						break;

					case "start":
						Minimum = reader.ReadContentAsDateTime2();
						break;

					case "end":
						Maximum = reader.ReadContentAsDateTime2();
						break;
				}
			}

			return true;
		}

		/// <summary>
		///     Saves the contents of this object into the given writer.
		/// </summary>
		/// <param name="writer"></param>
		public void Save(XmlWriter writer)
		{
			writer.WriteAttributeString("mode", Mode.ToString());
			writer.WriteAttributeString("range", SpecialInterval.ToString());
			if (Minimum != null)
				writer.WriteAttributeDateTime("start", Minimum.Value);
			if (Maximum != null)
				writer.WriteAttributeDateTime("end", Maximum.Value);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[Pure]
		public ILogEntryFilter CreateFilter()
		{
			// 
			IExpression<IInterval<DateTime?>> interval = TryCreateInterval();
			if (interval == null)
				return null;

			var expression = new ContainsTimestampExpression(interval, new TimestampVariable());
			return new FilterExpression(expression);
		}

		private IExpression<IInterval<DateTime?>> TryCreateInterval()
		{
			switch (Mode)
			{
				case TimeFilterMode.Everything:
					return null;

				case TimeFilterMode.SpecialInterval:
					return new DateTimeIntervalLiteral(SpecialInterval);

				case TimeFilterMode.Interval:
					return new DateTimeInterval(Minimum, Maximum);

				default:
					return null;
			}
		}
	}
}