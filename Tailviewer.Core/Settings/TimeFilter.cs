using System;
using System.Diagnostics.Contracts;
using System.Xml;
using Metrolib;
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
		/// </summary>
		public DateTime? End;

		/// <summary>
		///     One of the pre-defined special ranges (such as today).
		/// </summary>
		/// <remarks>
		///     In case this is set to a non-null value, then both <see cref="Start" />
		///     and <see cref="End" /> can be ignored: Instead the actual range must be evaluated
		///     and changed whenever applicable (i.e. filtering by Today must be re-evaluated on/after
		///     midnight, etc...
		/// </remarks>
		/// <remarks>
		///     Even when this is set to a non-null value, both <see cref="Start" /> and <see cref="End" />
		///     are expected to NOT be changed so we do not lose the user's most recent settings!
		/// </remarks>
		public SpecialTimeRange? Range;

		/// <summary>
		/// </summary>
		public DateTime? Start;

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[Pure]
		public TimeFilter Clone()
		{
			return new TimeFilter
			{
				Range = Range,
				Start = Start,
				End = End
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
					case "range":
						if (Enum.TryParse(reader.Value, ignoreCase: true, result: out SpecialTimeRange range))
							Range = range;
						break;

					case "start":
						Start = reader.ReadContentAsDateTime2();
						break;

					case "end":
						End = reader.ReadContentAsDateTime2();
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
			writer.WriteAttributeString("range", Range?.ToString() ?? string.Empty);
			if (Start != null)
				writer.WriteAttributeDateTime("start", Start.Value);
			if (End != null)
				writer.WriteAttributeDateTime("end", End.Value);
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

			var expression = new ContainsTimestampExpression(new TimestampVariable(), interval);
			return new FilterExpression(expression);
		}

		private IExpression<IInterval<DateTime?>> TryCreateInterval()
		{
			switch (Range)
			{
				case SpecialTimeRange.Today:
					return new DateTimeIntervalLiteral(DateTimeInterval.Today);

				// TODO: Add other values...
			}

			// TODO: Check start / end
			return null;
		}
	}
}