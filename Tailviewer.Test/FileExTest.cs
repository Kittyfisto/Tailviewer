using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core;

namespace Tailviewer.Test
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
		[Ignore("Behaviour isn't implemented yet")]
		public void TestGetFolderPaths5()
		{
			FileEx.GetFolderPaths(@"\\servername").Should().Equal(new object[] {"\\servername"});
		}
	}
}
