using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Xml;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Settings;

namespace Tailviewer.Test.Settings
{
	[TestFixture]
	public sealed class DataSourceSettingsTest
	{
		public static string[] Patterns => new []
		{
			"", "a", "*.log"
		};

		[Test]
		public void TestRestoreEmpty()
		{
			using (var stream = new MemoryStream())
			{
				using (var writer = XmlWriter.Create(stream))
				{
					writer.WriteStartElement("datasources");
					writer.WriteEndElement();
				}

				stream.Position = 0;

				using (var reader = XmlReader.Create(stream))
				{
					reader.MoveToContent();

					var actualSettings = new DataSourceSettings();
					actualSettings.Restore(reader, out _);
					actualSettings.FolderDataSourceRecursive.Should().BeTrue();
					actualSettings.FolderDataSourcePattern.Should().Be("*.txt;*.log");
				}
			}
		}

		[Test]
		public void TestClone([Values(true, false)] bool recursive,
							 [ValueSource(nameof(Patterns))] string pattern)
		{
			var id = DataSourceId.CreateNew();
			var dataSources = new DataSourceSettings
			{
				new DataSource(@"A:\stuff")
			};
			dataSources.FolderDataSourceRecursive = recursive;
			dataSources.SelectedItem = id;
			dataSources.FolderDataSourcePattern = pattern;

			var cloned = dataSources.Clone();
			cloned.Should().NotBeNull();
			cloned.Should().NotBeSameAs(dataSources);
			cloned.SelectedItem.Should().Be(id);
			cloned.FolderDataSourceRecursive.Should().Be(recursive);
			cloned.FolderDataSourcePattern.Should().Be(pattern);
			cloned.Count.Should().Be(1);
			cloned[0].Should().NotBeNull();
			cloned[0].Should().NotBeSameAs(dataSources[0]);
			cloned[0].File.Should().Be(@"A:\stuff");
		}

		[Test]
		public void TestMoveBefore1()
		{
			var dataSources = new DataSourceSettings();
			var a = new DataSource(@"A");
			var b = new DataSource(@"B");
			dataSources.Add(a);
			dataSources.Add(b);
			dataSources.MoveBefore(b, a);
			dataSources.Should().Equal(b, a);
		}

		[Test]
		[Description("Verifies that MoveBefore does not change the order of the list if the desired constraint already is true")]
		public void TestMoveBefore2()
		{
			var dataSources = new DataSourceSettings();
			var a = new DataSource(@"A");
			var b = new DataSource(@"B");
			dataSources.Add(a);
			dataSources.Add(b);
			dataSources.MoveBefore(a, b);
			dataSources.Should().Equal(a, b);
		}

		[Test]
		[Description("Verifies that MoveBefore does not change the order of the list if the desired constraint already is true")]
		public void TestMoveBefore3()
		{
			var dataSources = new DataSourceSettings();
			var a = new DataSource(@"A");
			var b = new DataSource(@"B");
			var c = new DataSource(@"C");
			dataSources.Add(a);
			dataSources.Add(b);
			dataSources.Add(c);
			dataSources.MoveBefore(a, c);
			dataSources.Should().Equal(a, b, c);
		}

		[Test]
		[Description("Verifies that MoveBefore doesn't do anything when either source or anchor don't exist")]
		public void TestMoveBefore4()
		{
			var dataSources = new DataSourceSettings();
			var a = new DataSource(@"A");
			var b = new DataSource(@"B");
			dataSources.Add(a);
			new Action(() => dataSources.MoveBefore(a, b)).Should().NotThrow();
			dataSources.Should().Equal(a);
			new Action(() => dataSources.MoveBefore(b, a)).Should().NotThrow();
			dataSources.Should().Equal(a);
		}

		[Test]
		[Description("Verifies that MoveBefore doesn't do anything when either source or anchor don't exist")]
		public void TestMoveBefore5()
		{
			var dataSources = new DataSourceSettings();
			var a = new DataSource(@"A");
			var b = new DataSource(@"B");
			var c = new DataSource(@"C");
			dataSources.Add(a);
			new Action(() => dataSources.MoveBefore(b, c)).Should().NotThrow();
			dataSources.Should().Equal(a);
		}

		[Test]
		public void TestRoundtrip([Values(true, false)] bool recursive,
						[ValueSource(nameof(Patterns))] string pattern)
		{
			var settings = new DataSourceSettings
			{
				FolderDataSourceRecursive = recursive,
				FolderDataSourcePattern = pattern
			};
			var actual = Roundtrip(settings);
			actual.FolderDataSourceRecursive.Should().Be(recursive);
			actual.FolderDataSourcePattern.Should().Be(pattern);
		}

		[Pure]
		private static DataSourceSettings Roundtrip(DataSourceSettings settings)
		{
			using (var stream = new MemoryStream())
			{
				using (var writer = XmlWriter.Create(stream))
				{
					writer.WriteStartElement("datasources");
					settings.Save(writer);
					writer.WriteEndElement();
				}

				stream.Position = 0;

				using (var reader = XmlReader.Create(stream))
				{
					reader.MoveToContent();

					var actualSettings = new DataSourceSettings();
					actualSettings.Restore(reader, out _);
					return actualSettings;
				}
			}
		}

	}
}