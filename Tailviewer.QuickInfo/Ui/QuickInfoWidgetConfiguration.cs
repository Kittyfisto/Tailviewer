using System;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.Ui.Analysis;

namespace Tailviewer.QuickInfo.Ui
{
	public sealed class QuickInfoWidgetConfiguration
		: IWidgetConfiguration
	{
		public QuickInfoWidgetConfiguration()
		{
			_titles = new Dictionary<Guid, QuickInfoViewConfiguration>();
		}

		public void Add(QuickInfoViewConfiguration config)
		{
			_titles.Add(config.Id, config);
		}

		public void Remove(Guid id)
		{
			_titles.Remove(id);
		}

		/// <summary>
		///     The title of the individual quick infos.
		/// </summary>
		/// <remarks>
		///     Is part of the view configuration because the title may be changed without analyzing again...
		/// </remarks>
		public IEnumerable<QuickInfoViewConfiguration> Titles => _titles.Values;

		private Dictionary<Guid, QuickInfoViewConfiguration> _titles;

		public object Clone()
		{
			return new QuickInfoWidgetConfiguration
			{
				_titles = _titles?.ToDictionary(x => x.Key, x=> x.Value?.Clone())
			};
		}

		public void Serialize(IWriter writer)
		{
			writer.WriteAttribute("Titles", Titles);
		}

		public void Deserialize(IReader reader)
		{
			var titles = new List<QuickInfoViewConfiguration>();
			if (reader.TryReadAttribute("Titles", titles))
			{
				_titles.Clear();
				foreach (var config in titles)
				{
					if (!_titles.ContainsKey(config.Id))
					{
						_titles.Add(config.Id, config);
					}
				}
			}
		}
	}
}