using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.Ui.Controls;
using Tailviewer.Ui.ViewModels;
using DataSource = Tailviewer.Settings.DataSource;

namespace Tailviewer.Test.Ui
{
	[TestFixture]
	public sealed class LogViewerControlTest
	{
		private LogViewerControl _control;

		[SetUp]
		[STAThread]
		public void SetUp()
		{
			_control = new LogViewerControl
				{
					DataSource = new SingleDataSourceViewModel(new SingleDataSource(new DataSource("Foobar"){Id = Guid.NewGuid()}))
				};
		}

		[Test]
		[STAThread]
		public void TestShowAll1()
		{
			_control.ShowAll = true;
			_control.ShowDebug.Should().BeTrue();
			_control.ShowInfo.Should().BeTrue();
			_control.ShowWarning.Should().BeTrue();
			_control.ShowError.Should().BeTrue();
			_control.ShowFatal.Should().BeTrue();
		}

		[Test]
		[STAThread]
		public void TestShowAll2()
		{
			_control.ShowDebug = true;
			_control.ShowInfo = true;
			_control.ShowWarning = true;
			_control.ShowError = true;
			_control.ShowFatal = true;
			_control.ShowAll = false;

			_control.ShowDebug.Should().BeFalse();
			_control.ShowInfo.Should().BeFalse();
			_control.ShowWarning.Should().BeFalse();
			_control.ShowError.Should().BeFalse();
			_control.ShowFatal.Should().BeFalse();
		}

		[Test]
		[STAThread]
		public void TestShowAll3()
		{
			_control.ShowAll = true;
			_control.ShowDebug = false;
			_control.ShowAll.Should().NotHaveValue();

			_control.ShowDebug.Should().BeFalse();
			_control.ShowInfo.Should().BeTrue();
			_control.ShowWarning.Should().BeTrue();
			_control.ShowError.Should().BeTrue();
			_control.ShowFatal.Should().BeTrue();
		}

		[Test]
		[STAThread]
		public void TestShowAll4()
		{
			_control.ShowAll = false;
			_control.ShowDebug = true;
			_control.ShowAll.Should().NotHaveValue();

			_control.ShowDebug.Should().BeTrue();
			_control.ShowInfo.Should().BeFalse();
			_control.ShowWarning.Should().BeFalse();
			_control.ShowError.Should().BeFalse();
			_control.ShowFatal.Should().BeFalse();
		}

		[Test]
		[STAThread]
		public void TestShowAll5()
		{
			_control.ShowAll = false;
			_control.ShowDebug = true;
			_control.ShowInfo = true;
			_control.ShowWarning = true;
			_control.ShowError = true;
			_control.ShowFatal = true;
			_control.ShowAll.Should().BeTrue();
		}

		[Test]
		[STAThread]
		public void TestCtor()
		{
			var source = new SingleDataSourceViewModel(new SingleDataSource(new DataSource("Foobar"){Id = Guid.NewGuid()}));
			source.LevelsFilter = LevelFlags.All;

			var control = new LogViewerControl
			{
				DataSource = source
			};
			control.ShowDebug.Should().BeTrue();
			control.ShowInfo.Should().BeTrue();
			control.ShowWarning.Should().BeTrue();
			control.ShowError.Should().BeTrue();
			control.ShowFatal.Should().BeTrue();
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