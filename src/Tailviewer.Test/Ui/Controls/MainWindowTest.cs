using System.Linq;
using System.Threading;
using System.Windows.Input;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Settings;
using Tailviewer.Ui;
using Tailviewer.Ui.Menu;

namespace Tailviewer.Test.Ui.Controls
{
	[TestFixture]
	[Apartment(ApartmentState.STA)]
	public sealed class MainWindowTest
	{
		private Mock<IApplicationSettings> _applicationSettings;
		private Mock<IMainWindowViewModel> _viewModel;
		private ObservableCollectionExt<KeyBindingCommand> _keyBindings;

		[SetUp]
		public void SetUp()
		{
			_applicationSettings = new Mock<IApplicationSettings>();
			_viewModel = new Mock<IMainWindowViewModel>();
			_keyBindings = new ObservableCollectionExt<KeyBindingCommand>();
			_viewModel.Setup(x => x.KeyBindings).Returns(_keyBindings);
			
		}

		[Test]
		public void TestConstructionInputBindings()
		{
			var command = new KeyBindingCommand(() => { })
			{
				GestureKey = Key.B, GestureModifier = ModifierKeys.Control | ModifierKeys.Alt
			};
			_keyBindings.Add(command);

			var control = CreateControl();
			control.InputBindings.Should().NotBeEmpty();
			var inputBinding = control.InputBindings.Cast<InputBinding>().First(x => x.Command == command);
			inputBinding.Command.Should().BeSameAs(command);
			inputBinding.Gesture.Should().BeOfType<KeyGesture>();
			var keyGesture = (KeyGesture) inputBinding.Gesture;
			keyGesture.Key.Should().Be(Key.B);
			keyGesture.Modifiers.Should().Be(ModifierKeys.Control | ModifierKeys.Alt);
		}

		[Test]
		public void TestClearInputBindings()
		{
			var command = new KeyBindingCommand(() => { })
			{
				GestureKey = Key.B, GestureModifier = ModifierKeys.Control | ModifierKeys.Alt
			};
			_keyBindings.Add(command);

			var control = CreateControl();
			control.InputBindings.Should().NotBeEmpty();
			var inputBinding = control.InputBindings.Cast<InputBinding>().First(x => x.Command == command);

			_keyBindings.Clear();
			control.InputBindings.Should().NotContain(inputBinding);
		}

		[Test]
		public void TestRemoveInputBindings()
		{
			var command1 = new KeyBindingCommand(() => { })
			{
				GestureKey = Key.B, GestureModifier = ModifierKeys.Control | ModifierKeys.Alt
			};
			_keyBindings.Add(command1);
			var command2 = new KeyBindingCommand(() => { })
			{
				GestureKey = Key.A, GestureModifier = ModifierKeys.Control | ModifierKeys.Alt
			};
			_keyBindings.Add(command2);

			var control = CreateControl();
			control.InputBindings.Should().NotBeEmpty();
			var inputBinding1 = control.InputBindings.Cast<InputBinding>().First(x => x.Command == command1);
			var inputBinding2 = control.InputBindings.Cast<InputBinding>().First(x => x.Command == command2);

			_keyBindings.Remove(command1);
			control.InputBindings.Should().NotContain(inputBinding1);
			control.InputBindings.Should().Contain(inputBinding2);

			_keyBindings.Remove(command2);
			control.InputBindings.Should().NotContain(inputBinding1);
			control.InputBindings.Should().NotContain(inputBinding2);
		}

		private MainWindow CreateControl()
		{
			var control = new MainWindow(_applicationSettings.Object, _viewModel.Object);
			DispatcherExtensions.ExecuteAllEvents();
			return control;
		}
	}
}
