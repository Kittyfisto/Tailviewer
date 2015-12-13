using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.Ui.Controls;

namespace Tailviewer.Test.Ui
{
	[TestFixture]
	public sealed class LevelFilterControlTest
	{
		private LevelFilterControl _control;

		[SetUp]
		[STAThread]
		public void SetUp()
		{
			_control = new LevelFilterControl();
		}

		[Test]
		[STAThread]
		public void TestChangeShowDebug()
		{
			_control.ShowDebug.Should().BeFalse();
			_control.LevelsFilter.Should().Be(LevelFlags.None);

			_control.ShowDebug = true;
			_control.LevelsFilter.Should().Be(LevelFlags.Debug);

			_control.ShowDebug = false;
			_control.LevelsFilter.Should().Be(LevelFlags.None);
		}

		[Test]
		[STAThread]
		public void TestChangeShowInfo()
		{
			_control.ShowInfo.Should().BeFalse();
			_control.LevelsFilter.Should().Be(LevelFlags.None);

			_control.ShowInfo = true;
			_control.LevelsFilter.Should().Be(LevelFlags.Info);

			_control.ShowInfo = false;
			_control.LevelsFilter.Should().Be(LevelFlags.None);
		}

		[Test]
		[STAThread]
		public void TestChangeShowWarning()
		{
			_control.ShowWarning.Should().BeFalse();
			_control.LevelsFilter.Should().Be(LevelFlags.None);

			_control.ShowWarning = true;
			_control.LevelsFilter.Should().Be(LevelFlags.Warning);

			_control.ShowWarning = false;
			_control.LevelsFilter.Should().Be(LevelFlags.None);
		}

		[Test]
		[STAThread]
		public void TestChangeShowError()
		{
			_control.ShowError.Should().BeFalse();
			_control.LevelsFilter.Should().Be(LevelFlags.None);

			_control.ShowError = true;
			_control.LevelsFilter.Should().Be(LevelFlags.Error);

			_control.ShowError = false;
			_control.LevelsFilter.Should().Be(LevelFlags.None);
		}

		[Test]
		[STAThread]
		public void TestChangeShowFatal()
		{
			_control.ShowFatal.Should().BeFalse();
			_control.LevelsFilter.Should().Be(LevelFlags.None);

			_control.ShowFatal = true;
			_control.LevelsFilter.Should().Be(LevelFlags.Fatal);

			_control.ShowFatal = false;
			_control.LevelsFilter.Should().Be(LevelFlags.None);
		}

		[Test]
		[STAThread]
		public void TestChangeLevelAll()
		{
			_control.LevelsFilter = LevelFlags.All;
			_control.ShowDebug.Should().BeTrue();
			_control.ShowInfo.Should().BeTrue();
			_control.ShowWarning.Should().BeTrue();
			_control.ShowError.Should().BeTrue();
			_control.ShowFatal.Should().BeTrue();
		}

		[Test]
		[STAThread]
		public void TestChangeLevelDebug()
		{
			_control.LevelsFilter = LevelFlags.Debug;
			_control.ShowDebug.Should().BeTrue();
			_control.ShowInfo.Should().BeFalse();
			_control.ShowWarning.Should().BeFalse();
			_control.ShowError.Should().BeFalse();
			_control.ShowFatal.Should().BeFalse();
		}

		[Test]
		[STAThread]
		public void TestChangeLevelInfo()
		{
			_control.LevelsFilter = LevelFlags.Info;
			_control.ShowDebug.Should().BeFalse();
			_control.ShowInfo.Should().BeTrue();
			_control.ShowWarning.Should().BeFalse();
			_control.ShowError.Should().BeFalse();
			_control.ShowFatal.Should().BeFalse();
		}

		[Test]
		[STAThread]
		public void TestChangeLevelWarning()
		{
			_control.LevelsFilter = LevelFlags.Warning;
			_control.ShowDebug.Should().BeFalse();
			_control.ShowInfo.Should().BeFalse();
			_control.ShowWarning.Should().BeTrue();
			_control.ShowError.Should().BeFalse();
			_control.ShowFatal.Should().BeFalse();
		}

		[Test]
		[STAThread]
		public void TestChangeLevelError()
		{
			_control.LevelsFilter = LevelFlags.Error;
			_control.ShowDebug.Should().BeFalse();
			_control.ShowInfo.Should().BeFalse();
			_control.ShowWarning.Should().BeFalse();
			_control.ShowError.Should().BeTrue();
			_control.ShowFatal.Should().BeFalse();
		}

		[Test]
		[STAThread]
		public void TestChangeLevelFatal()
		{
			_control.LevelsFilter = LevelFlags.Fatal;
			_control.ShowDebug.Should().BeFalse();
			_control.ShowInfo.Should().BeFalse();
			_control.ShowWarning.Should().BeFalse();
			_control.ShowError.Should().BeFalse();
			_control.ShowFatal.Should().BeTrue();
		}
	}
}