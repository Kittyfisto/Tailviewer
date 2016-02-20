using System;

namespace Tailviewer.BusinessLogic.LogFiles
{
	[Flags]
	public enum LevelFlags : byte
	{
		Fatal   = 0x01,
		Error   = 0x02,
		Warning = 0x04,
		Info    = 0x08,
		Debug   = 0x10,

		All = Fatal | Error | Warning | Info | Debug,
		None = 0
	}
}