using System;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.BusinessLogic.Analysis;

namespace Tailviewer.Analysis.QuickInfo.BusinessLogic
{
	public sealed class QuickInfoAnalyserConfiguration
		: ILogAnalyserConfiguration
	{
		private readonly Dictionary<Guid, QuickInfoConfiguration> _quickInfos;

		public QuickInfoAnalyserConfiguration()
		{
			_quickInfos = new Dictionary<Guid, QuickInfoConfiguration>();
		}

		private QuickInfoAnalyserConfiguration(Dictionary<Guid, QuickInfoConfiguration> quickInfos)
		{
			_quickInfos = quickInfos;
		}

		public IEnumerable<QuickInfoConfiguration> QuickInfos => _quickInfos.Values;

		public object Clone()
		{
			return new QuickInfoAnalyserConfiguration(_quickInfos?.ToDictionary(x => x.Key, x => x.Value?.Clone()));
		}

		public bool IsEquivalent(ILogAnalyserConfiguration other)
		{
			return false;
		}

		public void Serialize(IWriter writer)
		{
			writer.WriteAttribute("QuickInfos", QuickInfos);
		}

		public void Deserialize(IReader reader)
		{
			var tmp = new List<QuickInfoConfiguration>();
			if (reader.TryReadAttribute("QuickInfos", tmp))
			{
				_quickInfos.Clear();
				foreach (var info in tmp)
				{
					if (!_quickInfos.ContainsKey(info.Id))
					{
						_quickInfos.Add(info.Id, info);
					}
				}
			}
		}

		public void Add(QuickInfoConfiguration quickInfoConfiguration)
		{
			_quickInfos.Add(quickInfoConfiguration.Id, quickInfoConfiguration);;
		}

		public void Remove(Guid id)
		{
			_quickInfos.Remove(id);
		}
	}
}