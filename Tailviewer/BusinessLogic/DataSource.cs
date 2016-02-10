namespace Tailviewer.BusinessLogic
{
	internal sealed class SingleDataSource
		: AbstractDataSource
{
		private readonly LogFile _logFile;

		public SingleDataSource(Settings.DataSource settings)
			: base(settings)
		{
			_logFile = new LogFile(settings.File);
			_logFile.Start();
		}

		public override ILogFile LogFile
		{
			get { return _logFile; }
		}
}
}