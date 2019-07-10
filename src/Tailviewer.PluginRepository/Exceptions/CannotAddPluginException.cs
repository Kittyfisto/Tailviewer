using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tailviewer.PluginRegistry.Exceptions
{
	public class CannotAddPluginException
		: Exception
	{
		public CannotAddPluginException(string message)
			: base(message)
		{ }
	}
}
