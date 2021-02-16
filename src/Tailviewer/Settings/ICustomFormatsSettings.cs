﻿using System.Collections.Generic;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Plugins;
using Tailviewer.Settings.CustomFormats;

namespace Tailviewer.Settings
{
	/// <summary>
	///     Provides access to all <see cref="ICustomLogFileFormat" /> registered to this application.
	/// </summary>
	public interface ICustomFormatsSettings
		: IList<CustomLogFileFormat>
	{
	}
}