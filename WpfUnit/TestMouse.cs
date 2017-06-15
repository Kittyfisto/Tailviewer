using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Harmony;

namespace WpfUnit
{
	/// <summary>
	/// </summary>
	/// <remarks>
	///     Given the singleton nature of the <see cref="Mouse" /> class, you should NOT enable
	///     parallel tests for your controls when using this class.
	/// </remarks>
	/// <example>
	///     Each test method should create its own <see cref="TestMouse" /> instance in order
	///     to not be influenced by previous test methods (that may have left keys in a pressed state).
	/// </example>
	public sealed class TestMouse
	{
		private static readonly Dictionary<IInputElement, Point> Positions;

		static TestMouse()
		{
			Positions = new Dictionary<IInputElement, Point>();

			AssemblySetup.EnsureIsPatched();
		}

		/// <summary>
		///     Initializes the <see cref="Mouse" /> to its default state:
		///     It's relative position to every <see cref="IInputElement" /> is (0, 0)
		///     and all buttons are unpressed.
		/// </summary>
		public TestMouse()
		{
			Reset();
		}

		/// <summary>
		///     Resets the <see cref="Mouse" /> to its original state.
		/// </summary>
		private void Reset()
		{
			Positions.Clear();
		}

		/// <summary>
		///     Causes the mouse to change its relative position to the given element.
		///     After this method has been called, <see cref="Mouse.GetPosition(IInputElement)" />
		///     returns the given value.
		/// </summary>
		/// <param name="element"></param>
		/// <param name="relativePosition"></param>
		public void SetMousePositionRelativeTo(IInputElement element, Point relativePosition)
		{
			Positions[element] = relativePosition;
		}

		/// <summary>
		///     Causes the mouse to change its relative position to the given element
		///     and raises the <see cref="UIElement.MouseMoveEvent" /> on it.
		/// </summary>
		/// <param name="element"></param>
		/// <param name="relativePosition"></param>
		public void MoveRelativeTo(UIElement element, Point relativePosition)
		{
			SetMousePositionRelativeTo(element, relativePosition);
			element.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, Environment.TickCount)
			{
				RoutedEvent = UIElement.MouseMoveEvent
			});
		}

		/// <summary>
		///     Raises the <see cref="UIElement.MouseLeftButtonDownEvent" />
		///     followed by the <see cref="UIElement.MouseLeftButtonUpEvent" />.
		/// </summary>
		/// <param name="element"></param>
		public void LeftClick(UIElement element)
		{
			element.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
			{
				RoutedEvent = UIElement.MouseLeftButtonDownEvent
			});
			element.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
			{
				RoutedEvent = UIElement.MouseLeftButtonUpEvent
			});
		}

		/// <summary>
		///     Causes the mouse to change its relative position to the given element
		///     and raises the <see cref="UIElement.MouseMoveEvent" />, followed by the
		///     <see cref="UIElement.MouseLeftButtonDownEvent" /> and the
		///     <see cref="UIElement.MouseLeftButtonUpEvent" />.
		/// </summary>
		/// <param name="element"></param>
		/// <param name="relativePosition"></param>
		public void LeftClickAt(UIElement element, Point relativePosition)
		{
			MoveRelativeTo(element, relativePosition);
			LeftClick(element);
		}

		/// <summary>
		///     Raises the <see cref="UIElement.MouseRightButtonDownEvent" />
		///     followed by the <see cref="UIElement.MouseRightButtonUpEvent" />.
		/// </summary>
		/// <param name="element"></param>
		public void RightClick(UIElement element)
		{
			element.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Right)
			{
				RoutedEvent = UIElement.MouseRightButtonDownEvent
			});
			element.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Right)
			{
				RoutedEvent = UIElement.MouseRightButtonUpEvent
			});
		}

		/// <summary>
		///     Causes the mouse to change its relative position to the given element
		///     and raises the <see cref="UIElement.MouseMoveEvent" />, followed by the
		///     <see cref="UIElement.MouseRightButtonDownEvent" /> and the
		///     <see cref="UIElement.MouseRightButtonUpEvent" />.
		/// </summary>
		/// <param name="element"></param>
		/// <param name="relativePosition"></param>
		public void RightClickAt(UIElement element, Point relativePosition)
		{
			MoveRelativeTo(element, relativePosition);
			RightClick(element);
		}

		[HarmonyPatch(typeof(Mouse))]
		[HarmonyPatch("GetPosition")]
		static class PatchMouseGetPosition
		{
			private static void Postfix(IInputElement relativeTo, ref Point __result)
			{
				Positions.TryGetValue(relativeTo, out __result);
			}
		}

		[HarmonyPatch(typeof(MouseEventArgs))]
		[HarmonyPatch("GetPosition")]
		sealed class PatchMouseEventArgsGetPosition
		{
			public static void Postfix(IInputElement relativeTo, ref Point __result)
			{
				Positions.TryGetValue(relativeTo, out __result);
			}
		}
	}
}