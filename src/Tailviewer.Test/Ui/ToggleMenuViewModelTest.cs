using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Ui.Menu;

namespace Tailviewer.Test.Ui
{
	[TestFixture]
	public sealed class ToggleMenuViewModelTest
	{
		[Test]
		public void TestConstruction([Values(true, false)] bool isChecked)
		{
			var item = new ToggleMenuViewModel(isChecked, null);
			item.IsCheckable.Should().BeTrue();
			item.IsChecked.Should().Be(isChecked);
			if (isChecked)
				item.Icon.Should().NotBeNull();
			else
				item.Icon.Should().BeNull();
		}

		[Test]
		public void TestChangeIsChecked()
		{
			var item = new ToggleMenuViewModel(false, null);
			item.IsChecked = true;
			item.Icon.Should().NotBeNull();

			item.IsChecked = false;
			item.Icon.Should().BeNull();
		}
	}
}
