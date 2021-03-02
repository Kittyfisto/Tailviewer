using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;
using SharpRemote;
using Tailviewer.Archiver.Plugins;
using Tailviewer.Archiver.Repository;
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
			var warnings = new Dictionary<Type, ExitCode>
			{
				{typeof(PluginAlreadyPublishedException), ExitCode.PluginAlreadyPublished}
			};
			var errors = new Dictionary<Type, ExitCode>
			{
				{typeof(RemotePublishDisabledException), ExitCode.RemotePublishDisabled},
				{typeof(CorruptPluginException), ExitCode.CorruptPlugin},
				{typeof(InvalidUserTokenException), ExitCode.InvalidUserToken},
				{typeof(DirectoryNotFoundException), ExitCode.DirectoryNotFound },
				{typeof(FileNotFoundException), ExitCode.FileNotFound },
				{typeof(NoSuchIPEndPointException), ExitCode.ConnectionError }
			};

			try
			{
				var fileNames = ResolveFileNames(options.Plugin);
				if (fileNames.Count == 0)
				{
					Log.ErrorFormat("Did not find any file matching '{0}'!", options.Plugin);
					return ExitCode.FileNotFound;
				}

				LogFileNamesToPublish(fileNames);

				using (var client = new SocketEndPoint(EndPointType.Client))
				{
					Connect(client, options.Repository);

					var repository = client.CreateProxy<IPluginRepository>(Constants.PluginRepositoryV1Id);

					foreach (var fileName in fileNames)
					{
						var plugin = File.ReadAllBytes(fileName);
						repository.PublishPlugin(plugin, options.AccessToken);
					}

					Log.InfoFormat("Plugin successfully published!");
					return ExitCode.Success;
				}
			}
			catch (Exception e)
			{
				if (warnings.TryGetValue(e.GetType(), out var exitCode))
				{
					Log.WarnFormat(e.Message);
					Log.Debug(e); //< Stacktrace is literally of no interest to anyone unless something needs to be debugged
					return exitCode;
				}

				if (errors.TryGetValue(e.GetType(), out exitCode))
				{
					Log.ErrorFormat(e.Message);
					Log.Debug(e); //< Stacktrace is literally of no interest to anyone unless something needs to be debugged
					return exitCode;
				}

				Log.Error(e);
				return ExitCode.UnhandledException;
			}
		}

		private static void LogFileNamesToPublish(IReadOnlyList<string> fileNames)
		{
			var builder = new StringBuilder();
			builder.AppendFormat("Publishing {0} plugin(s):", fileNames.Count);
			foreach (var fileName in fileNames)
			{
				builder.AppendLine();
				builder.AppendFormat("\t{0}", fileName);
			}

			Log.Info(builder);
		}

		private IReadOnlyList<string> ResolveFileNames(string pluginPathOrPattern)
		{
			if (!pluginPathOrPattern.Contains("*"))
				return new[] {pluginPathOrPattern};

			var path = Path.GetDirectoryName(pluginPathOrPattern);
			if (string.IsNullOrWhiteSpace(path))
				path = Directory.GetCurrentDirectory();

			var pattern = Path.GetFileName(pluginPathOrPattern);
			return Directory.EnumerateFiles(path, pattern, SearchOption.TopDirectoryOnly).ToList();
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