using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace Tailviewer.AcceptanceTests
{
	[SetUpFixture]
	public sealed class AssemblySetup
	{
		[OneTimeSetUp]
		public void SetUp()
		{
			Directory.SetCurrentDirectory(AssemblyDirectory);
		}

		public static string AssemblyDirectory
		{
			get
			{
				string codeBase = Assembly.GetExecutingAssembly().CodeBase;
				UriBuilder uri = new UriBuilder(codeBase);
				string path = Uri.UnescapeDataString(uri.Path);
				return Path.GetDirectoryName(path);
			}
		}
	}
}