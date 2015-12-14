using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.Settings;
using Tailviewer.Ui.ViewModels;
using DataSource = Tailviewer.Settings.DataSource;

namespace Tailviewer.Test.Ui
{
	[TestFixture]
	public sealed class DataSourceViewModelTest
	{
		[Test]
		public void TestCtor()
		{
			using (var source = new Tailviewer.BusinessLogic.DataSource(new DataSource(@"E:\Code\SharpTail\SharpTail.Test\TestData\20Mb.test")))
			{
				var model = new DataSourceViewModel(source);
				model.FullName.Should().Be(@"E:\Code\SharpTail\SharpTail.Test\TestData\20Mb.test");
			}
		}

		[Test]
		public void TestRemoveCommand1()
		{
			using (var source = new Tailviewer.BusinessLogic.DataSource(new DataSource(@"E:\Code\SharpTail\SharpTail.Test\TestData\20Mb.test")))
			{
				var model = new DataSourceViewModel(source);
				model.RemoveCommand.Should().NotBeNull();
				model.RemoveCommand.CanExecute(null).Should().BeTrue();
				new Action(() => model.RemoveCommand.Execute(null)).ShouldNotThrow();
			}
		}

		[Test]
		public void TestRemoveCommand2()
		{
			using (var source = new Tailviewer.BusinessLogic.DataSource(new DataSource(@"E:\Code\SharpTail\SharpTail.Test\TestData\20Mb.test")))
			{
				var model = new DataSourceViewModel(source);
				var calls = new List<DataSourceViewModel>();
				model.Remove += calls.Add;
				new Action(() => model.RemoveCommand.Execute(null)).ShouldNotThrow();
				calls.Should().Equal(new[] { model });
			}
		}
	}
}