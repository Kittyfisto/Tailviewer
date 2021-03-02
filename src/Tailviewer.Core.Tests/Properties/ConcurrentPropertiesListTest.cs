using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Tailviewer.Api;

namespace Tailviewer.Core.Tests.Properties
{
	[TestFixture]
	public sealed class ConcurrentPropertiesListTest
		: AbstractPropertiesBufferTest
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