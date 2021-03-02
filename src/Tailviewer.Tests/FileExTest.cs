using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;

namespace Tailviewer.Tests
{
	[TestFixture]
	public sealed class FileExTest
	{
		[Test]
		public void TestGetFolderPaths1()
		{
			FileEx.GetFolderPaths(null).Should().BeEmpty();
			FileEx.GetFolderPaths("").Should().BeEmpty();
		}

		[Test]
		public void TestGetFolderPaths2()
		{
			FileEx.GetFolderPaths(@"C:\foo\bar\stuff.blub")
				.Should().Equal(new object[]
				{
					@"C:\foo\bar\stuff.blub",
					@"C:\foo\bar\",
					@"C:\foo\",
					@"C:\"
				});
		}

		[Test]
		public void TestGetFolderPaths3()
		{
			FileEx.GetFolderPaths(@"C:\foo\bar\stuff")
				.Should().Equal(new object[]
				{
					@"C:\foo\bar\stuff",
					@"C:\foo\bar\",
					@"C:\foo\",
					@"C:\"
				});
		}

		[Test]
		public void TestGetFolderPaths4()
		{
			FileEx.GetFolderPaths(@"C:\foo\bar\stuff\")
				.Should().Equal(new object[]
				{
					@"C:\foo\bar\stuff\",
					@"C:\foo\bar\",
					@"C:\foo\",
					@"C:\"
				});
		}

		[Test]
		public void TestGetFolderPaths5()
		{
			FileEx.GetFolderPaths(@"\\servername").Should().Equal(new object[] {@"\\servername"});
		}

		[Test]
		public void TestGetFolderPath6()
		{
			FileEx.GetFolderPaths(@"\\servername\folder1\foo")
				.Should().Equal(new object[]
				{
					@"\\servername\folder1\foo",
					@"\\servername\folder1\",
					@"\\servername\"
				});
		}

		[Test]
		public void TestGetDriveLetterFromThePath1()
		{
			string absolutePath;
			var drive = FileEx.GetDriveLetterFromPath(@"C:\Test", out absolutePath);
			drive.Should().NotBeNull();
			drive.Should().Be('C');
		}

		[Test]
		public void TestGetDriveLetterFromThePath2()
		{
			string absolutePath;
			var drive = FileEx.GetDriveLetterFromPath(@"\\servername", out absolutePath);
			drive.Should().BeNull();
			absolutePath.Should().Be(@"\\servername", "because it is a UNC path");
		}
	}
}
