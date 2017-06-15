using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using FluentAssertions;
using NUnit.Framework;

namespace WpfUnit.Test
{
	[TestFixture]
	[RequiresThread(ApartmentState.STA)]
	public sealed class TestKeyboardTest
	{
		public static IEnumerable<Key> Keys => Enum.GetValues(typeof(Key)).Cast<Key>().Skip(1).ToList();

		#region KeyboardDevice

		[Test]
		public void TestKeyboardDeviceIsKeyDown([ValueSource(nameof(Keys))] Key key)
		{
			var device = Keyboard.PrimaryDevice;

			var keyboard = new TestKeyboard();
			device.IsKeyDown(key).Should().BeFalse();

			keyboard.Press(key);
			device.IsKeyDown(key).Should().BeTrue();

			keyboard.Release(key);
			device.IsKeyDown(key).Should().BeFalse();
		}

		[Test]
		public void TestKeyboardDeviceIsKeyUp([ValueSource(nameof(Keys))] Key key)
		{
			var device = Keyboard.PrimaryDevice;

			var keyboard = new TestKeyboard();
			device.IsKeyUp(key).Should().BeTrue();

			keyboard.Press(key);
			device.IsKeyUp(key).Should().BeFalse();

			keyboard.Release(key);
			device.IsKeyUp(key).Should().BeTrue();
		}

		[Test]
		public void TestKeyboardDeviceModifiersShift()
		{
			var device = Keyboard.PrimaryDevice;

			var keyboard = new TestKeyboard();
			device.Modifiers.Should().Be(ModifierKeys.None);

			keyboard.Press(Key.LeftShift);
			device.Modifiers.Should().Be(ModifierKeys.Shift);

			keyboard.Release(Key.LeftShift);
			device.Modifiers.Should().Be(ModifierKeys.None);

			keyboard.Press(Key.RightShift);
			device.Modifiers.Should().Be(ModifierKeys.Shift);
		}

		[Test]
		public void TestKeyboardDeviceModifiersControl()
		{
			var device = Keyboard.PrimaryDevice;

			var keyboard = new TestKeyboard();
			device.Modifiers.Should().Be(ModifierKeys.None);

			keyboard.Press(Key.LeftCtrl);
			device.Modifiers.Should().Be(ModifierKeys.Control);

			keyboard.Release(Key.LeftCtrl);
			device.Modifiers.Should().Be(ModifierKeys.None);

			keyboard.Press(Key.RightCtrl);
			device.Modifiers.Should().Be(ModifierKeys.Control);
		}

		[Test]
		public void TestKeyboardDeviceModifiersAlt()
		{
			var device = Keyboard.PrimaryDevice;

			var keyboard = new TestKeyboard();
			device.Modifiers.Should().Be(ModifierKeys.None);

			keyboard.Press(Key.LeftAlt);
			device.Modifiers.Should().Be(ModifierKeys.Alt);

			keyboard.Release(Key.LeftAlt);
			device.Modifiers.Should().Be(ModifierKeys.None);

			keyboard.Press(Key.RightAlt);
			device.Modifiers.Should().Be(ModifierKeys.Alt);
		}

		#endregion

		#region Keyboard

		[Test]
		public void TestKeyboardIsKeyDown1()
		{
			Keyboard.IsKeyDown(Key.LeftShift).Should().BeFalse();

			var keyboard = new TestKeyboard();
			keyboard.Press(Key.LeftShift);
			Keyboard.IsKeyDown(Key.LeftShift).Should().BeTrue();
			keyboard.Release(Key.LeftShift);
			Keyboard.IsKeyDown(Key.LeftShift).Should().BeFalse();
		}

		[Test]
		public void TestIsKeyUp1()
		{
			Keyboard.IsKeyUp(Key.LeftShift).Should().BeTrue();

			var keyboard = new TestKeyboard();
			keyboard.Press(Key.LeftShift);
			Keyboard.IsKeyUp(Key.LeftShift).Should().BeFalse();
			keyboard.Release(Key.LeftShift);
			Keyboard.IsKeyUp(Key.LeftShift).Should().BeTrue();
		}

		[Test]
		public void TestIsKeyToggled1()
		{
			
		}

		[Test]
		[Description("Verifies that only the pressed keys are actually pressed")]
		public void TestPress1()
		{
			var keyboard = new TestKeyboard();

			foreach (var key in Keys)
			{
				Keyboard.IsKeyDown(key).Should().BeFalse();
				Keyboard.IsKeyUp(key).Should().BeTrue();
				Keyboard.IsKeyToggled(key).Should().BeFalse();
			}

			keyboard.Press(Key.LeftShift);

			foreach (var key in Keys)
			{
				if (key == Key.LeftShift)
				{
					Keyboard.IsKeyDown(key).Should().BeTrue();
					Keyboard.IsKeyUp(key).Should().BeFalse();
				}
				else
				{
					Keyboard.IsKeyDown(key).Should().BeFalse();
					Keyboard.IsKeyUp(key).Should().BeTrue();
				}
				Keyboard.IsKeyToggled(key).Should().BeFalse();
			}
		}

		#endregion
	}
}
