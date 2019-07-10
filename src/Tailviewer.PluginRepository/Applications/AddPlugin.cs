﻿using System.Reflection;
using log4net;
using Tailviewer.PluginRepository.Exceptions;

namespace Tailviewer.PluginRepository.Applications
{
	public static class AddPlugin
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static int Run(AddPluginOptions options)
		{
			using (var repo = new PluginRepository())
			{
				try
				{
					repo.AddPlugin(options.PluginFileName, options.AccessToken);
				}
				catch (CannotAddPluginException e)
				{
					Log.ErrorFormat(e.Message);
					return -10;
				}

				return 0;
			}
		}
	}
}