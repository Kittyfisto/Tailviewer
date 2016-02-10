using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Ui.ViewModels;
using DataSource = Tailviewer.Settings.DataSource;

namespace Tailviewer.Test.Ui
{
	[TestFixture]
	public sealed class SingleDataSourceViewModelTest
	{
		[Test]
		public void TestCtor()
		{
			using (var source = new Tailviewer.BusinessLogic.SingleDataSource(new DataSource(@"E:\Code\SharpTail\SharpTail.Test\TestData\20Mb.test")))
			{
				var model = new SingleDataSourceViewModel(source);
				model.FullName.Should().Be(@"E:\Code\SharpTail\SharpTail.Test\TestData\20Mb.test");
			}
		}

		[Test]
		public void TestRemoveCommand1()
		{
			using (var source = new Tailviewer.BusinessLogic.SingleDataSource(new DataSource(@"E:\Code\SharpTail\SharpTail.Test\TestData\20Mb.test")))
			{
				var model = new SingleDataSourceViewModel(source);
				model.RemoveCommand.Should().NotBeNull();
				model.RemoveCommand.CanExecute(null).Should().BeTrue();
				new Action(() => model.RemoveCommand.Execute(null)).ShouldNotThrow();
			}
		}

		[Test]
		public void TestRemoveCommand2()
		{
			using (var source = new Tailviewer.BusinessLogic.SingleDataSource(new DataSource(@"E:\Code\SharpTail\SharpTail.Test\TestData\20Mb.test")))
			{
				var model = new SingleDataSourceViewModel(source);
				var calls = new List<IDataSourceViewModel>();
				model.Remove += calls.Add;
				new Action(() => model.RemoveCommand.Execute(null)).ShouldNotThrow();
				calls.Should().Equal(new[] { model });
			}
		}
	}
}