using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.Settings;
using Tailviewer.Ui.Controls;
using Tailviewer.Ui.ViewModels;

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
			_control = new LevelFilterControl
				{
					DataSource = new DataSourceViewModel(new DataSource(new DataSourceSettings("Foobar")))
				};
		}

		[Test]
		[STAThread]
		public void TestCtor()
		{
			var source = new DataSourceViewModel(new DataSource(new DataSourceSettings("Foobar")));
			source.OtherFilter = true;
			source.LevelsFilter = LevelFlags.All;

			var control = new LevelFilterControl
			{
				DataSource = source
			};
			control.ShowOther.Should().BeFalse();
			control.ShowDebug.Should().BeTrue();
			control.ShowInfo.Should().BeTrue();
			control.ShowWarning.Should().BeTrue();
			control.ShowError.Should().BeTrue();
			control.ShowFatal.Should().BeTrue();
		}

		[Test]
		[STAThread]
		public void TestChangeShowOther()
		{
			_control.DataSource.OtherFilter = true;
			_control.ShowOther.Should().BeFalse();

			_control.ShowOther = true;
			_control.DataSource.OtherFilter.Should().BeFalse();

			_control.ShowOther = false;
			_control.DataSource.OtherFilter.Should().BeTrue();
		}

		[Test]
		[STAThread]
		public void TestChangeShowDebug()
		{
			_control.DataSource.LevelsFilter = LevelFlags.None;
			_control.ShowDebug.Should().BeFalse();

			_control.ShowDebug = true;
			_control.DataSource.LevelsFilter.Should().Be(LevelFlags.Debug);

			_control.ShowDebug = false;
			_control.DataSource.LevelsFilter.Should().Be(LevelFlags.None);
		}

		[Test]
		[STAThread]
		public void TestChangeShowInfo()
		{
			_control.DataSource.LevelsFilter = LevelFlags.None;
			_control.ShowInfo.Should().BeFalse();

			_control.ShowInfo = true;
			_control.DataSource.LevelsFilter.Should().Be(LevelFlags.Info);

			_control.ShowInfo = false;
			_control.DataSource.LevelsFilter.Should().Be(LevelFlags.None);
		}

		[Test]
		[STAThread]
		public void TestChangeShowWarning()
		{
			_control.DataSource.LevelsFilter = LevelFlags.None;
			_control.ShowWarning.Should().BeFalse();

			_control.ShowWarning = true;
			_control.DataSource.LevelsFilter.Should().Be(LevelFlags.Warning);

			_control.ShowWarning = false;
			_control.DataSource.LevelsFilter.Should().Be(LevelFlags.None);
		}

		[Test]
		[STAThread]
		public void TestChangeShowError()
		{
			_control.DataSource.LevelsFilter = LevelFlags.None;
			_control.ShowError.Should().BeFalse();

			_control.ShowError = true;
			_control.DataSource.LevelsFilter.Should().Be(LevelFlags.Error);

			_control.ShowError = false;
			_control.DataSource.LevelsFilter.Should().Be(LevelFlags.None);
		}

		[Test]
		[STAThread]
		public void TestChangeShowFatal()
		{
			_control.DataSource.LevelsFilter = LevelFlags.None;
			_control.ShowFatal.Should().BeFalse();

			_control.ShowFatal = true;
			_control.DataSource.LevelsFilter.Should().Be(LevelFlags.Fatal);

			_control.ShowFatal = false;
			_control.DataSource.LevelsFilter.Should().Be(LevelFlags.None);
		}

		[Test]
		[STAThread]
		public void TestChangeLevelAll()
		{
			_control.DataSource.LevelsFilter = LevelFlags.All;
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
			_control.DataSource.LevelsFilter = LevelFlags.Debug;
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
			_control.DataSource.LevelsFilter = LevelFlags.Info;
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
			_control.DataSource.LevelsFilter = LevelFlags.Warning;
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
			_control.DataSource.LevelsFilter = LevelFlags.Error;
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
			_control.DataSource.LevelsFilter = LevelFlags.Fatal;
			_control.ShowDebug.Should().BeFalse();
			_control.ShowInfo.Should().BeFalse();
			_control.ShowWarning.Should().BeFalse();
			_control.ShowError.Should().BeFalse();
			_control.ShowFatal.Should().BeTrue();
		}
	}
}