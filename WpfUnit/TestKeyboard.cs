using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Harmony;

namespace WpfUnit
{
	/// <summary>
	///     This class can be used to control the <see cref="Keyboard" /> class in a test scenario.
	///     By default, all keys are not pressed until <see cref="Press(Key)" /> is called.
	/// </summary>
	/// <remarks>
	///     Given the singleton nature of the <see cref="Keyboard" /> class, you should NOT enable
	///     parallel tests for your controls when using this class.
	/// </remarks>
	/// <example>
	///     Each test method should create its own <see cref="TestKeyboard" /> instance in order
	///     to not be influenced by previous test methods (that may have left keys in a pressed state).
	/// </example>
	public sealed class TestKeyboard
	{
		private static readonly HashSet<Key> PressedKeys;
		private readonly HwndSource _dummyInputSource;

		static TestKeyboard()
		{
			PressedKeys = new HashSet<Key>();

			AssemblySetup.EnsureIsPatched();
		}

		/// <summary>
		///     Initializes a new TestKeyboard.
		///     Defaults all keys to not being pressed.
		/// </summary>
		public TestKeyboard()
		{
			_dummyInputSource = new HwndSource(0, 0, 0, 0, 0, "", IntPtr.Zero);
			Reset();
		}

		/// <summary>
		///     Defaults all keys to not being pressed.
		/// </summary>
		public void Reset()
		{
			PressedKeys.Clear();
		}

		public void Press(Key key)
		{
			PressedKeys.Add(key);
		}

		public void Release(Key key)
		{
			PressedKeys.Remove(key);
		}

		public void Click(UIElement element, Key key)
		{
			Press(key);
			element.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice,
				_dummyInputSource,
				Environment.TickCount,
				key)
			{
				RoutedEvent = UIElement.KeyDownEvent
			});

			Release(key);
			element.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice,
				_dummyInputSource,
				Environment.TickCount,
				key)
			{
				RoutedEvent = UIElement.KeyUpEvent
			});
		}

		#region Keyboard

		[HarmonyPatch(typeof(Keyboard))]
		[HarmonyPatch("IsKeyDown")]
		private class PatchIsKeyDown
		{
			private static void Postfix(Key key, ref bool __result)
			{
				__result = PressedKeys.Contains(key);
			}
		}

		[HarmonyPatch(typeof(Keyboard))]
		[HarmonyPatch("IsKeyUp")]
		private class PatchIsKeyUp
		{
			private static void Postfix(Key key, ref bool __result)
			{
				__result = !PressedKeys.Contains(key);
			}
		}

		[HarmonyPatch(typeof(Keyboard))]
		[HarmonyPatch("IsKeyToggled")]
		private class PatchIsKeyToggled
		{
			private static void Postfix(Key key, ref bool __result)
			{
				__result = false;
			}
		}

		#endregion

		#region KeyboardDevice

		[HarmonyPatch(typeof(KeyboardDevice))]
		[HarmonyPatch("IsKeyDown")]
		private class PatchKeyboardDeviceIsKeyDown
		{
			private static void Postfix(Key key, ref bool __result)
			{
				__result = PressedKeys.Contains(key);
			}
		}

		[HarmonyPatch(typeof(KeyboardDevice))]
		[HarmonyPatch("IsKeyUp")]
		private class PatchKeyboardDeviceIsKeyUp
		{
			private static void Postfix(Key key, ref bool __result)
			{
				__result = !PressedKeys.Contains(key);
			}
		}

		[HarmonyPatch(typeof(KeyboardDevice))]
		[HarmonyPatch("get_Modifiers")]
		private class PatchKeyboardDeviceModifiers
		{
			private static void Postfix(ref ModifierKeys __result)
			{
				var modifiers = ModifierKeys.None;
				if (PressedKeys.Contains(Key.LeftShift) || PressedKeys.Contains(Key.RightShift))
					modifiers |= ModifierKeys.Shift;
				if (PressedKeys.Contains(Key.LeftCtrl) || PressedKeys.Contains(Key.RightCtrl))
					modifiers |= ModifierKeys.Control;
				if (PressedKeys.Contains(Key.LeftAlt) || PressedKeys.Contains(Key.RightAlt))
					modifiers |= ModifierKeys.Alt;
				__result = modifiers;
			}
		}

		#endregion
	}
}