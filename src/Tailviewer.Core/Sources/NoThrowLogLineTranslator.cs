using System;
using System.Reflection;
using log4net;

namespace Tailviewer.Core.Sources
{
	/// <summary>
	///     Wraps a given <see cref="ILogLineTranslator" /> and ensures that any exceptions thrown
	///     by it are handled.
	/// </summary>
	public sealed class NoThrowLogLineTranslator
		: ILogLineTranslator
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ILogLineTranslator _translator;

		/// <summary>
		///     Initalizes this object.
		/// </summary>
		/// <param name="translator"></param>
		public NoThrowLogLineTranslator(ILogLineTranslator translator)
		{
			if (translator == null)
				throw new ArgumentNullException(nameof(translator));

			_translator = translator;
		}

		/// <inheritdoc />
		public LogLine Translate(ILogSource logSource, LogLine line)
		{
			try
			{
				return _translator.Translate(logSource, line);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
				return line;
			}
		}
	}
}