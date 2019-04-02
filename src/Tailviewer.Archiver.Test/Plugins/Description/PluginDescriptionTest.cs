using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Archiver.Plugins.Description;

namespace Tailviewer.Archiver.Test.Plugins.Description
{
	[TestFixture]
	public sealed class PluginDescriptionTest
	{
		[Test]
		public void TestToString()
		{
			new PluginDescription().ToString().Should().Be("<Unknown>");

			new PluginDescription
			{
				Name = "My First Plugin"
			}.ToString().Should().Be("Name: My First Plugin");

			new PluginDescription
			{
				Version = new Version(1, 2, 3, 4)
			}.ToString().Should().Be("Version: 1.2.3.4");

			new PluginDescription
			{
				Author = "Simon"
			}.ToString().Should().Be("Author: Simon");
		}
	}
}
