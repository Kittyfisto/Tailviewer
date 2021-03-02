using System;

namespace Tailviewer.Api
{
	/// <summary>
	///     This attribute indicates to users of an interface that a method is thread-safe and may be safely called from
	///     multiple threads without requiring any additional synchronization.
	///     Likewise, this attributes tells to developers of an interface that the method *must* be implemented so that it can be safely
	///     called from multiple threads. If that requires synchronization, then so be it. If it can be achieved without
	///     synchronization (because the method is pure and doesn't require any state whatsoever), then even better.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public sealed class ThreadSafeAttribute
		: Attribute
	{}
}