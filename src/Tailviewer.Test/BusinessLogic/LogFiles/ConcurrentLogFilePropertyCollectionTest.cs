using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Tailviewer.Core.Properties;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class ConcurrentLogFilePropertyCollectionTest
		: AbstractLogFilePropertiesTest
	{
		#region Overrides of AbstractLogFilePropertiesTest

		protected override IPropertiesBuffer Create(params KeyValuePair<IReadOnlyPropertyDescriptor, object>[] properties)
		{
			var collection = new ConcurrentPropertiesList(properties.Select(x => x.Key));
			foreach (var pair in properties)
			{
				collection.SetValue(pair.Key, pair.Value);
			}

			return collection;
		}

		#endregion
	}
}