using System;

namespace Tailviewer
{
	/// <summary>
	///     This attribute marks interfaces which can be retrieved from a <see cref="IServiceContainer" />.
	/// </summary>
	[AttributeUsage(AttributeTargets.Interface)]
	public sealed class ServiceAttribute
		: Attribute
	{
	}
}