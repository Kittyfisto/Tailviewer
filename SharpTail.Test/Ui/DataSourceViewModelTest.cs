using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using SharpTail.BusinessLogic;
using SharpTail.Ui.ViewModels;

namespace SharpTail.Test.Ui
{
	[TestFixture]
	public sealed class DataSourceViewModelTest
	{
		[Test]
		public void TestCtor()
		{
			var now = DateTime.Now;
			var source = new DataSource(@"E:\Code\SharpTail\SharpTail.Test\TestData\20Mb.test")
				{
					LastWritten = now
				};
			var model = new DataSourceViewModel(source);
			model.LastWritten.Should().Be(now);
		}

		[Test]
		public void TestRemoveCommand1()
		{
			var source = new DataSource(@"E:\Code\SharpTail\SharpTail.Test\TestData\20Mb.test");
			var model = new DataSourceViewModel(source);
			model.RemoveCommand.Should().NotBeNull();
			model.RemoveCommand.CanExecute(null).Should().BeTrue();
			new Action(() => model.RemoveCommand.Execute(null)).ShouldNotThrow();
		}

		[Test]
		public void TestRemoveCommand2()
		{
			var source = new DataSource(@"E:\Code\SharpTail\SharpTail.Test\TestData\20Mb.test");
			var model = new DataSourceViewModel(source);
			var calls = new List<DataSourceViewModel>();
			model.Remove += calls.Add;
			new Action(() => model.RemoveCommand.Execute(null)).ShouldNotThrow();
			calls.Should().Equal(new[] {model});
		}
	}
}