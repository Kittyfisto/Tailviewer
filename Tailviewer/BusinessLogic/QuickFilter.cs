using System;
using System.Diagnostics.Contracts;
using Tailviewer.Settings;

namespace Tailviewer.BusinessLogic
{
	internal sealed class QuickFilter
	{
		private readonly Settings.QuickFilter _settings;

		public QuickFilter(Settings.QuickFilter settings)
		{
			if (settings == null) throw new ArgumentNullException("settings");

			_settings = settings;
		}

		public string Value
		{
			get { return _settings.Value; }
			set { _settings.Value = value; }
		}

		public bool IgnoreCase
		{
			get { return _settings.IgnoreCase; }
			set { _settings.IgnoreCase = value; }
		}

		public QuickFilterType Type
		{
			get { return _settings.Type; }
			set { _settings.Type = value; }
		}

		public Guid Id
		{
			get { return _settings.Id; }
		}

		[Pure]
		public IFilter CreateFilter()
		{
			var value = Value;
			switch (Type)
			{
				case QuickFilterType.StringFilter:
					if (!string.IsNullOrEmpty(value))
						return new SubstringFilter(value, IgnoreCase);
					break;

				case QuickFilterType.WildcardFilter:
					if (!string.IsNullOrEmpty(value))
						return new WildcardFilter(value, IgnoreCase);
						
					break;
			}

			return null;
		}
	}
}