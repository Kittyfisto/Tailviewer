using System;

namespace Tailviewer.BusinessLogic.ActionCenter
{
	public sealed class Build
		: INotification
	{
		public static readonly Build Current;

		static Build()
		{
			Current = new Build(Constants.BuildDate, Constants.ApplicationVersion);
		}

		public Build(DateTime buildDate, Version applicationVersion)
		{
			Title = string.Format("{0} {1}, built on {2:D}, {3:T}",
				Constants.ApplicationTitle,
				applicationVersion,
				buildDate,
				buildDate);
		}

		public string Title { get; }

		public bool ForceShow => false;
	}
}