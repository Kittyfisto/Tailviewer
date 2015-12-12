using System;
using FluentAssertions;
using NUnit.Framework;
using SharpTail.BusinessLogic;
using SharpTail.Ui.ViewModels;

namespace SharpTail.Test
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
	}
}