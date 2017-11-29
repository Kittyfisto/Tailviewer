using System;

namespace TablePlayground
{
	public sealed class LogColumn<T>
		: ILogColumn<T>
	{
		public Type DataType => typeof(T);
	}
}