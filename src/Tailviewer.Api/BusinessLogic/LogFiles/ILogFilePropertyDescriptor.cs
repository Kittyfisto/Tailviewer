namespace Tailviewer.BusinessLogic.LogFiles
{
	/// <summary>
	///    Describes a property of an <see cref="ILogFile"/> which may be changed by the end-user.
	/// </summary>
	public interface ILogFilePropertyDescriptor
		: IReadOnlyPropertyDescriptor
	{
		
	}

	/// <summary>
	///    Describes a property of an <see cref="ILogFile"/> which may be changed by the end-user.
	/// </summary>
	public interface IPropertyDescriptor<out T>
		: IReadOnlyPropertyDescriptor<T>
		, ILogFilePropertyDescriptor
	{
		
	}
}