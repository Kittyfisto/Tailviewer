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
				string start = null;
				if (options.SinceLastTag)
					start = FindFirstCommit(options);

				var output = GetGitLog(options, start);
				var changes = Parse(output);
				var filtered = Filter(changes, options.Include, options.Exclude);
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
		private static string FindFirstCommit(GitGetChangesOptions options)
		{
			using (var process = new Process())
			{
				var argumentBuilder = new StringBuilder();
				argumentBuilder.Append("describe --tags --long");
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
					throw new Exception($"git describe returned {exitCode}");

				return GetCommitAfterLastTag(output);
			}
		}

		[Pure]
		public static string GetCommitAfterLastTag(string output)
		{
			var regex = new Regex(@"-(\d+)-g[0-9a-f]{6,40}");
			var match = regex.Match(output);
			if (match.Groups.Count != 2)
				throw new Exception($"Unable to parse git describe output: {output}");

			var distance = int.Parse(match.Groups[1].Value) - 2;
			return $"HEAD~{distance}";
		}

		[Pure]
		private static string GetGitLog(GitGetChangesOptions options, string startCommit)
		{
			using (var process = new Process())
			{
				var argumentBuilder = new StringBuilder();
				argumentBuilder.Append("log");
				if (!string.IsNullOrEmpty(startCommit))
					argumentBuilder.AppendFormat(" {0}..HEAD", startCommit);
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
			var regex = new Regex(@"commit ([0-9a-f]{40})\s*(\(tag:\s+([^)]+)\)){0,1}\nAuthor:[^\n]+\nDate:[^\n]+", RegexOptions.Multiline);
			var matches = regex.Matches(output);
			var changes = new SerializableChanges();

			for (int i = 0; i < matches.Count - 1; ++i)
			{
				var match = matches[i];
				var nextMatch = matches[i + 1];

				var id = match.Groups[1].Value;
				var start = match.Index + match.Length;
				var end = nextMatch.Index;
				var message = output.Substring(start, end - start);
				TryAddChange(id, message, changes);
			}

			if (matches.Count > 0)
			{
				var lastMatch = matches[matches.Count - 1];
				var id = lastMatch.Groups[1].Value;
				var start = lastMatch.Index + lastMatch.Length;
				var message = output.Substring(start);
				TryAddChange(id, message, changes);
			}

			return changes;
		}

		private static void TryAddChange(string id, string message, SerializableChanges changes)
		{
			var split = message.Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);

			if (split.Length > 0)
			{
				var change = new SerializableChange
				{
					Id = id,
					Summary = split[0].Trim()
				};
				change.Description = string.Join(Environment.NewLine, split.Skip(1).Select(x => x.Trim()));

				changes.Changes.Add(change);
			}
		}

		[Pure]
		public static SerializableChanges Filter(SerializableChanges changes, string include, string exclude)
		{
			var filtered = new SerializableChanges();
			filtered.Changes.AddRange(changes.Changes.Where(x => Matches(x, include, exclude)));
			return filtered;
		}

		[Pure]
		private static bool Matches(SerializableChange serializableChange, string include, string exclude)
		{
			if (!string.IsNullOrEmpty(include) && !Matches(serializableChange, include))
				return false;

			if (!string.IsNullOrEmpty(exclude) && Matches(serializableChange, exclude))
				return false;

			return true;
		}

		[Pure]
		private static bool Matches(SerializableChange serializableChange, string phrase)
		{
			if (serializableChange.Summary.IndexOf(phrase, StringComparison.InvariantCultureIgnoreCase) != -1)
				return true;

			if (string.IsNullOrEmpty(serializableChange.Description))
				return false;

			return serializableChange.Description.IndexOf(phrase, StringComparison.InvariantCultureIgnoreCase) != -1;
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
