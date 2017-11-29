using System;

namespace TablePlayground
{
	public interface ILogColumn
	{
		Type DataType { get; }
	}

	public interface ILogColumn<T>
		: ILogColumn
	{
		
	}
}