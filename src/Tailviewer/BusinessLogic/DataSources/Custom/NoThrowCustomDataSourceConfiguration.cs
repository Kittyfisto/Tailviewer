using System;
using System.Reflection;
using System.Xml;
using log4net;
using Tailviewer.Api;

namespace Tailviewer.BusinessLogic.DataSources.Custom
{
	public class NoThrowCustomDataSourceConfiguration
		: ICustomDataSourceConfiguration
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public readonly ICustomDataSourceConfiguration Inner;

		public NoThrowCustomDataSourceConfiguration(ICustomDataSourcePlugin inner, IServiceContainer serviceContainer)
		{
			try
			{
				Inner = inner.CreateConfiguration(serviceContainer);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
			}
		}

		private NoThrowCustomDataSourceConfiguration(ICustomDataSourceConfiguration clone)
		{
			Inner = clone;
		}

		#region Implementation of ICloneable

		public object Clone()
		{
			try
			{
				var clone = (ICustomDataSourceConfiguration)Inner?.Clone();
				return new NoThrowCustomDataSourceConfiguration(clone);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
				return new NoThrowCustomDataSourceConfiguration(null);
			}
		}

		#endregion

		#region Implementation of ICustomDataSourceConfiguration

		public void Restore(XmlReader reader)
		{
			try
			{
				Inner?.Restore(reader);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
			}
		}

		public void Store(XmlWriter writer)
		{
			try
			{
				Inner?.Store(writer);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
			}
		}

		#endregion
	}
}