using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Ui.Controls.MainPanel.About;

namespace Tailviewer.Test.Ui.Controls.About
{
	[TestFixture]
	public sealed class ChangelogViewModelTest
	{
		[Test]
		public void TestCtor()
		{
			var model = new ChangelogViewModel();
			model.Changes.Count().Should().Be(Changelog.Changes.Count());
		}
	}
}
