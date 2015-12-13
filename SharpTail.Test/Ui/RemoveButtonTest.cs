using System;
using FluentAssertions;
using NUnit.Framework;
using SharpTail.Ui.Controls;

namespace SharpTail.Test.Ui
{
	[TestFixture]
	public sealed class RemoveButtonTest
	{
		private RemoveButton _button;

		[SetUp]
		[STAThread]
		public void SetUp()
		{
			_button = new RemoveButton();
		}

		[Test]
		[STAThread]
		public void TestCtor()
		{
			_button.IsInverted.Should().BeFalse();
			_button.IsWhite.Should().BeFalse();
			_button.IsBlack.Should().BeTrue();
		}

		[Test]
		[STAThread]
		public void TestChangeInverted()
		{
			_button.IsInverted = true;
			_button.IsWhite.Should().BeTrue();
			_button.IsBlack.Should().BeFalse();

			_button.IsInverted = false;
			_button.IsWhite.Should().BeFalse();
			_button.IsBlack.Should().BeTrue();
		}
	}
}