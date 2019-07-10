using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Tailviewer.PluginRepository.Exceptions;

namespace Tailviewer.PluginRepository.Applications
{
	public sealed class RemovePlugin
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static int Run(RemovePluginOptions options)
		{
			using (var repo = new PluginRepository())
			{
				try
				{
					repo.RemovePlugin(options.Id, options.Version);
				}
				catch (CannotRemovePluginException e)
				{
					Log.ErrorFormat(e.Message);
					return -10;
				}

				return 0;
			}
		}
	}
}
