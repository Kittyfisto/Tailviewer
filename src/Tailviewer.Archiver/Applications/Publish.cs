using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net;
using System.Reflection;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;
using SharpRemote;
using Tailviewer.Archiver.Plugins;
using Tailviewer.Archiver.Repository;
using Tailviewer.Core;
using Constants = Tailviewer.Archiver.Repository.Constants;
using Exception = System.Exception;

namespace Tailviewer.Archiver.Applications
{
	public sealed class Publish
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public Publish()
		{
			// Depending on the options, some messages should be visible to the user (i.e. written to the console).
			var hierarchy = (Hierarchy)LogManager.GetRepository();
			var consoleAppender = new ColoringConsoleAppender(logTimestamps: false);
			hierarchy.Root.AddAppender(consoleAppender);
			hierarchy.Root.Level = Level.Info;
			hierarchy.Configured = true;
		}

		public ExitCode Run(PublishOptions options)
		{
			if (!TryLoadPlugin(options, out var plugin, out var exitCode))
				return exitCode;

			var exceptions = new Dictionary<Type, ExitCode>
			{
				{typeof(RemotePublishDisabledException), ExitCode.RemotePublishDisabled},
				{typeof(CorruptPluginException), ExitCode.CorruptPlugin},
				{typeof(InvalidUserTokenException), ExitCode.InvalidUserToken},
				{typeof(PluginAlreadyPublishedException), ExitCode.PluginAlreadyPublished}
			};

			using (var client = new SocketEndPoint(EndPointType.Client))
			{
				try
				{
					Connect(client, options.ServerAddress);

					var repository = client.CreateProxy<IPluginRepository>(Constants.PluginRepositoryV1Id);
					repository.PublishPlugin(plugin, options.AccessToken);

					Log.InfoFormat("Plugin successfully published!");
					return ExitCode.Success;
				}
				catch (Exception e)
				{
					if (exceptions.TryGetValue(e.GetType(), out exitCode))
					{
						Log.ErrorFormat(e.Message);
						Log.Debug(e); //< Stacktrace is literally of no interest to anyone unless something needs to be debugged
						return exitCode;
					}

					Log.Error(e);
					return ExitCode.UnhandledException;
				}
			}
		}

		private static bool TryLoadPlugin(PublishOptions options, out byte[] plugin, out ExitCode exitCode)
		{
			try
			{
				plugin = File.ReadAllBytes(options.Plugin);
				exitCode = ExitCode.Success;
				return true;
			}
			catch (FileNotFoundException e)
			{
				Log.ErrorFormat(e.Message);
				plugin = null;
				exitCode = ExitCode.FileNotFound;
				return false;
			}
			catch (DirectoryNotFoundException e)
			{
				Log.ErrorFormat(e.Message);
				plugin = null;
				exitCode = ExitCode.DirectoryNotFound;
				return false;
			}
		}

		private void Connect(SocketEndPoint client, string address)
		{
			var endPoint = ParseAddress(address);

			var hostEntry = Dns.GetHostEntry(endPoint.Host);
			var addresses = hostEntry.AddressList;
			if (addresses.Length == 0)
				throw new Exception($"Unable to find host '{endPoint.Host}'");

			Exception lastException = null;
			foreach (var ipAddress in addresses)
				if (client.TryConnect(new IPEndPoint(ipAddress, endPoint.Port), TimeSpan.FromSeconds(10), out lastException, out _))
					return;

			// ReSharper disable once PossibleNullReferenceException
			throw lastException;
		}

		[Pure]
		private static DnsEndPoint ParseAddress(string address)
		{
			string prefix = $"{Constants.Protocol}://";
			if (!address.StartsWith(prefix))
				throw new Exception($"Unsupported protocol: {address}");

			var uri = address.Substring(prefix.Length);
			var tokens = uri.Split(':');
			if (tokens.Length != 2)
				throw new Exception($"Expected an address in the form of {prefix}<hostname>:<port>");

			var hostname = tokens[0];
			var port = int.Parse(tokens[1]);
			var endPoint = new DnsEndPoint(hostname, port);
			return endPoint;
		}
	}
}