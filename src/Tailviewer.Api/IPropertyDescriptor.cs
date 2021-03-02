namespace Tailviewer.Api
{
	/// <summary>
	///    Describes a property of an <see cref="ILogSource"/> which may be changed by the end-user.
	/// </summary>
	public interface IPropertyDescriptor
		: IReadOnlyPropertyDescriptor
	{
		
	}

	/// <summary>
	///    Describes a property of an <see cref="ILogSource"/> which may be changed by the end-user.
	/// </summary>
	public interface IPropertyDescriptor<T>
		: IReadOnlyPropertyDescriptor<T>
		, IPropertyDescriptor
	{
		
	}
}