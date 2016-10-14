using System;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.BusinessLogic.ActionCenter;

namespace Tailviewer.Ui.ViewModels.ActionCenter
{
	public sealed class ChangeViewModel
		: AbstractNotificationViewModel
	{
		private readonly Change _change;

		public ChangeViewModel(Change change)
			:base(change)
		{
			if (change == null)
				throw new ArgumentNullException("change");

			_change = change;
		}

		public Version Version
		{
			get { return _change.Version; }
		}

		public bool HasFeatures
		{
			get { return Features.Any(); }
		}

		public bool HasBugfixes
		{
			get { return Bugfixes.Any(); }
		}

		public bool HasMisc
		{
			get { return Misc.Any(); }
		}

		public IEnumerable<string> Features
		{
			get { return _change.Features; }
		}

		public IEnumerable<string> Bugfixes
		{
			get { return _change.Bugfixes; }
		}

		public IEnumerable<string> Misc
		{
			get { return _change.Misc; }
		}
	}
}