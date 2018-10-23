using System;
using Tailviewer.Core.Settings;

namespace Tailviewer.QuickInfo.Ui
{
	public sealed class QuickInfoViewConfiguration
		: ISerializableType
		, ICloneable
	{
		public QuickInfoViewConfiguration()
		{
			_id = Guid.Empty;
			Name = "New Quick Info";
			Format = "{message}";
		}

		public QuickInfoViewConfiguration(Guid id)
			: this()
		{
			_id = id;
		}

		public Guid Id => _id;

		public string Name;

		/// <summary>
		///     An optional format string used to present matches from the filter.
		///     Is only used when <see cref="QuickFilter.MatchType" /> is set to
		///     <see cref="Core.Settings.FilterMatchType.RegexpFilter" />.
		/// </summary>
		/// <remarks>
		///     Shall be in the form of .NET format strings: "v{0}.{1}" uses
		///     the first two matches of the regular expression, etc...
		/// </remarks>
		public string Format;

		private Guid _id;

		public QuickInfoViewConfiguration Clone()
		{
			return new QuickInfoViewConfiguration
			{
				Name = Name,
				Format = Format
			};
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		#region Implementation of ISerializableType

		public void Serialize(IWriter writer)
		{
			writer.WriteAttribute("Id", _id);
			writer.WriteAttribute("Name", Name);
			writer.WriteAttribute("Format", Format);
		}

		public void Deserialize(IReader reader)
		{
			reader.TryReadAttribute("Id", out _id);
			reader.TryReadAttribute("Name", out Name);
			reader.TryReadAttribute("Format", out Format);
		}

		#endregion
	}
}