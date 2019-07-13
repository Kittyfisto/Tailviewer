using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;
using Tailviewer.Archiver.Plugins;
using Tailviewer.Core;

namespace Tailviewer.Archiver.Applications
{
	public sealed class GitGetChanges
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public GitGetChanges()
		{
			// Depending on the options, some messages should be visible to the user (i.e. written to the console).
			var hierarchy = (Hierarchy)LogManager.GetRepository();
			var consoleAppender = new ColoringConsoleAppender(logTimestamps: false);
			hierarchy.Root.AddAppender(consoleAppender);
			hierarchy.Root.Level = Level.Info;
			hierarchy.Configured = true;
		}

		public ExitCode Run(GitGetChangesOptions options)
		{
			try
			{
				var output = RunGitLog(options);
				var changes = Parse(output);
				var filtered = Filter(changes, options.Filter);
				WriteToDisk(options.Output, filtered);
			}
			catch (Exception e)
			{
				Log.ErrorFormat(e.Message);
				Log.Debug(e);
				return ExitCode.GenericFailure;
			}

			return ExitCode.Success;
		}

		[Pure]
		private static string RunGitLog(GitGetChangesOptions options)
		{
			using (var process = new Process())
			{
				var argumentBuilder = new StringBuilder();
				argumentBuilder.Append("log");
				/*if (!string.IsNullOrEmpty(options.Branch))
					argumentBuilder.AppendFormat(" --branches={0}", options.Branch);*/
				argumentBuilder.Append(" --pretty=medium");

				process.StartInfo = new ProcessStartInfo
				{
					CreateNoWindow = true,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					FileName = "git.exe",
					WorkingDirectory = options.Repository,
					Arguments = argumentBuilder.ToString()
				};

				process.Start();

				var output = process.StandardOutput.ReadToEnd();
				var error = process.StandardError.ReadToEnd();

				process.WaitForExit();
				var exitCode = process.ExitCode;
				if (exitCode != 0)
					throw new Exception($"git log returned {exitCode}");

				return output;
			}
		}

		[Pure]
		private static SerializableChanges Parse(string output)
		{
			var regex = new Regex(@"commit [0-9a-f]{40}\nAuthor:[^\n]+\nDate:[^\n]+", RegexOptions.Multiline);
			var matches = regex.Matches(output);
			var changes = new SerializableChanges();

			for (int i = 0; i < matches.Count - 1; ++i)
			{
				var match = matches[i];
				var nextMatch = matches[i + 1];

				var start = match.Index + match.Length;
				var end = nextMatch.Index;
				var message = output.Substring(start, end - start);
				var split = message.Split(new[]{'\n'}, StringSplitOptions.RemoveEmptyEntries);

				if (split.Length > 0)
				{
					var change = new SerializableChange
					{
						Summary = split[0].Trim()
					};
					change.Description = string.Join(Environment.NewLine, split.Skip(1).Select(x => x.Trim()));
					changes.Changes.Add(change);
				}
			}

			return changes;
		}

		[Pure]
		private static SerializableChanges Filter(SerializableChanges changes, string filter)
		{
			if (string.IsNullOrWhiteSpace(filter))
				return changes;

			var filtered = new SerializableChanges();
			filtered.Changes.AddRange(changes.Changes.Where(x => MatchesFilter(x, filter)));
			return filtered;
		}

		[Pure]
		private static bool MatchesFilter(SerializableChange serializableChange, string filter)
		{
			if (serializableChange.Summary.Contains(filter))
				return true;

			if (string.IsNullOrEmpty(serializableChange.Description))
				return false;

			return serializableChange.Description.Contains(filter);
		}

		private void WriteToDisk(string outputFileName, SerializableChanges changes)
		{
			using (var fileStream = File.Create(outputFileName))
			{
				changes.Serialize(fileStream);
			}
		}
	}
}
