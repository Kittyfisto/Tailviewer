using System.Runtime.Serialization;
using System.Windows;

namespace SharpTail
{
	/// <summary>
	///     Represents the configuration of a window.
	///     Currently only preserves a window's position + dimension, but may be expanded in the future.
	/// </summary>
	[DataContract]
	public sealed class WindowConfiguration
	{
		[DataMember(Order = 4)]
		public double Height;
		[DataMember(Order = 1)]
		public double Left;
		[DataMember(Order = 5)]
		public WindowState State;
		[DataMember(Order = 2)]
		public double Top;
		[DataMember(Order = 3)]
		public double Width;

		public void RestoreTo(Window window)
		{
			window.Left = Left;
			window.Top = Top;
			window.Width = Width;
			window.Height = Height;
			window.WindowState = State;
		}

		public static WindowConfiguration From(Window window)
		{
			return new WindowConfiguration
				{
					Left = window.Left,
					Top = window.Top,
					Width = window.Width,
					Height = window.Height,
					State = window.WindowState,
				};
		}
	}
}