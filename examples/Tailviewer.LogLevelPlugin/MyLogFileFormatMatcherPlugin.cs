using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Tailviewer.Api;
using Tailviewer.Core;

namespace Tailviewer.LogLevelPlugin
{
	/// <summary>
	///     This class is instantiated when this plugin is loaded by tailviewer and will be used to categorize log files into a particular
	///     <see cref="ILogFileFormat"/>. The first matcher which claims a <see cref="Certainty.Sure"/> detection wins and the format it
	///     detects will be locked in.
	/// </summary>
	/// <remarks>
	///     Other plugins will, in turn, be able to use the <see cref="Properties.Format"/> of an <see cref="ILogSource"/> to determine if they
	///     want to work with that log source or if they don't want to.
	/// </remarks>
	public sealed class MyLogFileFormatMatcherPlugin
		: ILogFileFormatMatcherPlugin
	{
		public static readonly ILogFileFormat MyCustomFormat;

		static MyLogFileFormatMatcherPlugin()
		{
			MyCustomFormat = new MyCustomLogFileFormat
			{
				Name = "Mylog v2"
			};
		}

		#region Implementation of ILogFileFormatMatcherPlugin

		public ILogFileFormatMatcher CreateMatcher(IServiceContainer services)
		{
			return new MyLogFileFormatMatcher();
		}

		#endregion

		private sealed class MyCustomLogFileFormat
			: ILogFileFormat
		{
			#region Implementation of ILogFileFormat

			public string Name { get; set; }

			public string Description { get; set; }

			public bool IsText => true;

			public Encoding Encoding => new UTF7Encoding();

			#endregion
		}

		private sealed class MyLogFileFormatMatcher
			: ILogFileFormatMatcher
		{
			#region Implementation of ILogFileFormatMatcher

			public bool TryMatchFormat(string fileName,
			                           byte[] data,
			                           Encoding encoding,
			                           out ILogFileFormat format,
			                           out Certainty certainty)
			{
				// In this example we are able to detect our log format by the custom extension
				// 'mylog' as well as by a file-name pattern "log_*.logv2". If the filename
				// matches either of these, then we assume that we're dealing with the desired
				// format and forward its descriptor to Tailviewer.
				// Other plugins may then compare log file format descriptors and thereby choose
				// which formats they want to be used for (and which ones they do not).

				var extension = Path.GetExtension(fileName);
				if (extension == "mylog")
				{
					format = MyCustomFormat;
					certainty = Certainty.Sure;
					return true;
				}

				var name = Path.GetFileName(fileName);
				if (Regex.Match(name, "log_*\\.logv2").Success)
				{
					format = MyCustomFormat;
					certainty = Certainty.Sure;
					return true;
				}

				// Since the filename doesn't match our expectations, we can say with 100% certainty
				// that we don't know what type of log file this is.
				// This causes Tailviewer to no longer invoke this matcher for that file anymore, even
				// if the file changes / is appended to / etc...
				format = null;
				certainty = Certainty.Sure;
				return false;
			}

			#endregion
		}
	}
}