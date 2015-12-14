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

		public StringComparison Comparison
		{
			get { return _settings.Comparison; }
			set { _settings.Comparison = value; }
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
			switch (Type)
			{
				case QuickFilterType.StringFilter:
					var value = Value;
					if (!string.IsNullOrEmpty(value))
						return new SubstringFilter(value, StringComparison.InvariantCultureIgnoreCase);
					break;
			}

			return null;
		}
	}
}