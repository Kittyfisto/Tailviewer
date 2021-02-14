using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class ConcurrentLogFilePropertyCollectionTest
		: AbstractLogFilePropertiesTest
	{
		#region Overrides of AbstractLogFilePropertiesTest

		protected override ILogFileProperties Create(params KeyValuePair<ILogFilePropertyDescriptor, object>[] properties)
		{
			var collection = new ConcurrentLogFilePropertyCollection(properties.Select(x => x.Key));
			foreach (var pair in properties)
			{
				collection.SetValue(pair.Key, pair.Value);
			}

			return collection;
		}

		#endregion
	}
}