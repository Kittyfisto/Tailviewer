using System;

namespace Tailviewer.Api
{
	/// <summary>
	///     This attribute marks interfaces which can be retrieved from a <see cref="IServiceContainer" />.
	/// </summary>
	/// <remarks>
	///     This attribute doesn't influence Tailviewer's behavior: It exists so plugin authors can search for the list
	///     of interfaces which are provided by the service container from within their IDE / in Tailviewer's source code.
	/// </remarks>
	[AttributeUsage(AttributeTargets.Interface)]
	public sealed class ServiceAttribute
		: Attribute
	{
	}
}