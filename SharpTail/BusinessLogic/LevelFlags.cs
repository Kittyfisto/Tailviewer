using System;

namespace SharpTail.BusinessLogic
{
	[Flags]
	public enum LevelFlags
	{
		Fatal   = 0x01,
		Error   = 0x02,
		Warning = 0x04,
		Info    = 0x08,
		Debug   = 0x10,

		All = Fatal | Error | Warning | Info | Debug
	}
}