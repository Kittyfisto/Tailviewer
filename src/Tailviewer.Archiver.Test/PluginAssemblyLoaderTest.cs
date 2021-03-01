using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using FluentAssertions;
using log4net.Core;
using NUnit.Framework;
using Tailviewer.Archiver.Plugins;
using Tailviewer.Archiver.Plugins.Description;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Plugins;
using Tailviewer.Test;

namespace Tailviewer.Archiver.Test
{
	[TestFixture]
	public sealed class PluginAssemblyLoaderTest
		: AbstractPluginTest
	{
		[Test]
		public void TestReflectPlugin1()
		{
			var plugin = "Kittyfisto.SomePlugin.dll";
			if (File.Exists(plugin))
				File.Delete(plugin);

			CreatePlugin(plugin, "John Snow", "https://got.com", "You know nothing, John Snow!");
			var scanner = new PluginAssemblyLoader();
			var description = scanner.ReflectPlugin(plugin);
			description.Should().NotBeNull();
			description.FilePath.Should().Be(plugin);
			description.Author.Should().Be("John Snow");
			description.Website.Should().Be(new Uri("https://got.com", UriKind.RelativeOrAbsolute));
			description.Description.Should().Be("You know nothing, John Snow!");
			description.PluginImplementations.Should().NotBeNull();
			description.PluginImplementations.Should().BeEmpty("Because we didn't add any plugin implementations");
		}

		[Test]
		public void TestReflectPlugin2()
		{
			var plugin = "sql.dll";
			if (File.Exists(plugin))
				File.Delete(plugin);

			var builder = new PluginBuilder("Simon", "sql", "sql", "SMI", "none of your business", "go away");
			builder.ImplementInterface<ILogEntryParserPlugin>("sql.LogFilePlugin");
			builder.Save();

			var scanner = new PluginAssemblyLoader();
			var description = scanner.ReflectPlugin(plugin);
			description.Author.Should().Be("SMI");
			description.Website.Should().Be(new Uri("none of your business", UriKind.RelativeOrAbsolute));
			description.Description.Should().Be("go away");
			description.PluginImplementations.Should().HaveCount(1);
			description.PluginImplementations.Should().Contain(x => x.InterfaceType == typeof(ILogEntryParserPlugin));
			var implementationDescription = description.PluginImplementations[0];
			implementationDescription.FullTypeName.Should().Be("sql.LogFilePlugin");
			implementationDescription
				.Version.Should().Be(PluginInterfaceVersionAttribute.GetInterfaceVersion(typeof(ILogEntryParserPlugin)));
		}

		[Test]
		public void TestReflectPlugin3()
		{
			var scanner = new PluginAssemblyLoader();
			new Action(() => scanner.ReflectPlugin(null)).Should().Throw<ArgumentNullException>();
		}

		[Test]
		public void TestReflectPlugin4()
		{
			var scanner = new PluginAssemblyLoader();
			new Action(() => scanner.ReflectPlugin("DAAWDADAWWF")).Should().Throw<FileNotFoundException>();
		}

		[Test]
		public void TestReflectPlugin5()
		{
			var scanner = new PluginAssemblyLoader();
			new Action(() => scanner.ReflectPlugin(@"C:\adwwdwawad\asxas")).Should().Throw<FileNotFoundException>();
		}

		[Test]
		public void TestReflectPlugin6()
		{
			var scanner = new PluginAssemblyLoader();
			new Action(() => scanner.ReflectPlugin("C:\adwwdwawad\asxas")).Should().Throw<ArgumentException>(
				"because we used illegal characters in that path");
		}

		#region DataContract checks

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/177")]
		public void TestReflectPluginMissingDefaultCtor()
		{
			var pluginBuilder = new PluginBuilder("Kittyfisto", "Test", "TestReflectPluginMissingDefaultCtor");
			var type = pluginBuilder.DefineType("SomeSerializableType", TypeAttributes.Class | TypeAttributes.Public);
			type.AddInterfaceImplementation(typeof(ISerializableType));
			var attribute = pluginBuilder.BuildCustomAttribute(new DataContractAttribute());
			type.SetCustomAttribute(attribute);
			var ctorBuilder = type.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, new[] {typeof(object)});
			var gen = ctorBuilder.GetILGenerator();
			gen.Emit(OpCodes.Ret);

			var serialize = type.DefineMethod(nameof(ISerializableType.Serialize), MethodAttributes.Public | MethodAttributes.Virtual,
			                                  CallingConventions.HasThis,
			                                  typeof(void),
			                                  new []{typeof(IWriter)});
			serialize.GetILGenerator().Emit(OpCodes.Ret);

			var deserialize = type.DefineMethod(nameof(ISerializableType.Deserialize), MethodAttributes.Public | MethodAttributes.Virtual,
			                                    CallingConventions.HasThis,
			                                    typeof(void),
			                                    new []{typeof(IReader)});
			deserialize.GetILGenerator().Emit(OpCodes.Ret);

			type.CreateType();
			pluginBuilder.Save();

			var scanner = new PluginAssemblyLoader();

			var appender = Appender.CaptureEvents("Tailviewer.Archiver.Plugins.PluginAssemblyLoader", Level.Error);
			scanner.ReflectPlugin(pluginBuilder.FileName);
			appender.Events.Should().HaveCount(1, "because one serializable type is missing a parameterless constructor and this should've provoked an error");
			var error = appender.Events.First();
			error.RenderedMessage.Should().Contain(type.FullName);
			error.RenderedMessage.Should().Contain("is missing a parameterless constructor, you must add one");
		}

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/177")]
		public void TestReflectPluginProtectedDefaultCtor()
		{
			var pluginBuilder = new PluginBuilder("Kittyfisto", "Test", "TestReflectPluginProtectedDefaultCtor");
			var type = pluginBuilder.DefineType("SomeSerializableType", TypeAttributes.Class | TypeAttributes.Public);
			type.AddInterfaceImplementation(typeof(ISerializableType));
			var attribute = pluginBuilder.BuildCustomAttribute(new DataContractAttribute());
			type.SetCustomAttribute(attribute);
			var ctorBuilder = type.DefineConstructor(MethodAttributes.Family, CallingConventions.HasThis, new Type[0]);
			var gen = ctorBuilder.GetILGenerator();
			gen.Emit(OpCodes.Ret);

			var serialize = type.DefineMethod(nameof(ISerializableType.Serialize), MethodAttributes.Public | MethodAttributes.Virtual,
			                                  CallingConventions.HasThis,
			                                  typeof(void),
			                                  new []{typeof(IWriter)});
			serialize.GetILGenerator().Emit(OpCodes.Ret);

			var deserialize = type.DefineMethod(nameof(ISerializableType.Deserialize), MethodAttributes.Public | MethodAttributes.Virtual,
			                                    CallingConventions.HasThis,
			                                    typeof(void),
			                                    new []{typeof(IReader)});
			deserialize.GetILGenerator().Emit(OpCodes.Ret);

			type.CreateType();
			pluginBuilder.Save();

			var scanner = new PluginAssemblyLoader();

			var appender = Appender.CaptureEvents("Tailviewer.Archiver.Plugins.PluginAssemblyLoader", Level.Error);
			scanner.ReflectPlugin(pluginBuilder.FileName);
			appender.Events.Should().HaveCount(1, "because the serializable type's parameterless constructor is not publicly visible and this should have provoked an error");
			var error = appender.Events.First();
			error.RenderedMessage.Should().Contain(type.FullName);
			error.RenderedMessage.Should().Contain("only has a protected parameterless constructor, you must set it to public!");
		}

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/177")]
		public void TestReflectPluginPrivateDefaultCtor()
		{
			var pluginBuilder = new PluginBuilder("Kittyfisto", "Test", "TestReflectPluginNonPublicDefaultCtor");
			var type = pluginBuilder.DefineType("SomeSerializableType", TypeAttributes.Class | TypeAttributes.Public);
			type.AddInterfaceImplementation(typeof(ISerializableType));
			var attribute = pluginBuilder.BuildCustomAttribute(new DataContractAttribute());
			type.SetCustomAttribute(attribute);
			var ctorBuilder = type.DefineConstructor(MethodAttributes.Private, CallingConventions.HasThis, new Type[0]);
			var gen = ctorBuilder.GetILGenerator();
			gen.Emit(OpCodes.Ret);

			var serialize = type.DefineMethod(nameof(ISerializableType.Serialize), MethodAttributes.Public | MethodAttributes.Virtual,
			                                  CallingConventions.HasThis,
			                                  typeof(void),
			                                  new []{typeof(IWriter)});
			serialize.GetILGenerator().Emit(OpCodes.Ret);

			var deserialize = type.DefineMethod(nameof(ISerializableType.Deserialize), MethodAttributes.Public | MethodAttributes.Virtual,
			                                    CallingConventions.HasThis,
			                                    typeof(void),
			                                    new []{typeof(IReader)});
			deserialize.GetILGenerator().Emit(OpCodes.Ret);

			type.CreateType();
			pluginBuilder.Save();

			var scanner = new PluginAssemblyLoader();

			var appender = Appender.CaptureEvents("Tailviewer.Archiver.Plugins.PluginAssemblyLoader", Level.Error);
			scanner.ReflectPlugin(pluginBuilder.FileName);
			appender.Events.Should().HaveCount(1, "because the serializable type's parameterless constructor is not publicly visible and this should have provoked an error");
			var error = appender.Events.First();
			error.RenderedMessage.Should().Contain(type.FullName);
			error.RenderedMessage.Should().Contain("only has a private parameterless constructor, you must set it to public!");
		}

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/177")]
		public void TestReflectPluginInternalDefaultCtor()
		{
			var pluginBuilder = new PluginBuilder("Kittyfisto", "Test", "TestReflectPluginInternalDefaultCtor");
			var type = pluginBuilder.DefineType("SomeSerializableType", TypeAttributes.Class | TypeAttributes.Public);
			type.AddInterfaceImplementation(typeof(ISerializableType));
			var attribute = pluginBuilder.BuildCustomAttribute(new DataContractAttribute());
			type.SetCustomAttribute(attribute);
			var ctorBuilder = type.DefineConstructor(MethodAttributes.Assembly, CallingConventions.HasThis, new Type[0]);
			var gen = ctorBuilder.GetILGenerator();
			gen.Emit(OpCodes.Ret);

			var serialize = type.DefineMethod(nameof(ISerializableType.Serialize), MethodAttributes.Public | MethodAttributes.Virtual,
			                                  CallingConventions.HasThis,
			                                  typeof(void),
			                                  new []{typeof(IWriter)});
			serialize.GetILGenerator().Emit(OpCodes.Ret);

			var deserialize = type.DefineMethod(nameof(ISerializableType.Deserialize), MethodAttributes.Public | MethodAttributes.Virtual,
			                                    CallingConventions.HasThis,
			                                    typeof(void),
			                                    new []{typeof(IReader)});
			deserialize.GetILGenerator().Emit(OpCodes.Ret);

			type.CreateType();
			pluginBuilder.Save();

			var scanner = new PluginAssemblyLoader();

			var appender = Appender.CaptureEvents("Tailviewer.Archiver.Plugins.PluginAssemblyLoader", Level.Error);
			scanner.ReflectPlugin(pluginBuilder.FileName);
			appender.Events.Should().HaveCount(1, "because the serializable type's parameterless constructor is not publicly visible and this should have provoked an error");
			var error = appender.Events.First();
			error.RenderedMessage.Should().Contain(type.FullName);
			error.RenderedMessage.Should().Contain("only has an internal parameterless constructor, you must set it to public!");
		}

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/177")]
		public void TestReflectPluginNonPublicSerializableType()
		{
			var pluginBuilder = new PluginBuilder("Kittyfisto", "Test", "TestReflectPluginNonPublicSerializableType");
			var type = pluginBuilder.DefineType("SomeSerializableType", TypeAttributes.Class | TypeAttributes.NotPublic);
			type.AddInterfaceImplementation(typeof(ISerializableType));
			var attribute = pluginBuilder.BuildCustomAttribute(new DataContractAttribute());
			type.SetCustomAttribute(attribute);
			var ctorBuilder = type.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, new Type[0]);
			var gen = ctorBuilder.GetILGenerator();
			gen.Emit(OpCodes.Ret);

			var serialize = type.DefineMethod(nameof(ISerializableType.Serialize), MethodAttributes.Public | MethodAttributes.Virtual,
			                                  CallingConventions.HasThis,
			                                  typeof(void),
			                                  new []{typeof(IWriter)});
			serialize.GetILGenerator().Emit(OpCodes.Ret);

			var deserialize = type.DefineMethod(nameof(ISerializableType.Deserialize), MethodAttributes.Public | MethodAttributes.Virtual,
			                                    CallingConventions.HasThis,
			                                    typeof(void),
			                                    new []{typeof(IReader)});
			deserialize.GetILGenerator().Emit(OpCodes.Ret);

			type.CreateType();
			pluginBuilder.Save();

			var scanner = new PluginAssemblyLoader();

			var appender = Appender.CaptureEvents("Tailviewer.Archiver.Plugins.PluginAssemblyLoader", Level.Error);
			scanner.ReflectPlugin(pluginBuilder.FileName);
			appender.Events.Should().HaveCount(1, "because the serializable type's parameterless constructor is not publicly visible and this should have provoked an error");
			var error = appender.Events.First();
			error.RenderedMessage.Should().Contain(type.FullName);
			error.RenderedMessage.Should().Contain("must be set to public!");
		}

		#endregion

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/178")]
		public void TestReflectTwoPluginImplementations()
		{
			var builder = new PluginBuilder("Kittyfisto", "TestReflectTwoPluginImplementations", "TestReflectTwoPluginImplementations");
			builder.ImplementInterface<ILogEntryParserPlugin>("A");
			builder.ImplementInterface<ILogEntryParserPlugin>("B");
			builder.Save();

			var assemblyLoader = new PluginAssemblyLoader();
			var description = assemblyLoader.ReflectPlugin(builder.FileName);
			description.PluginImplementations.Should().HaveCount(2, "because we've implemented the IFileFormatPlugin twice");
			description.PluginImplementations[0].InterfaceType.Should().Be<ILogEntryParserPlugin>();
			description.PluginImplementations[0].FullTypeName.Should().Be("A");

			description.PluginImplementations[1].InterfaceType.Should().Be<ILogEntryParserPlugin>();
			description.PluginImplementations[1].FullTypeName.Should().Be("B");
		}

		[Test]
		public void TestReflectPlugins()
		{
			var scanner = new PluginAssemblyLoader(@"C:\adwwdwawad\asxas");
			scanner.Plugins.Should().BeEmpty();
		}

		[Test]
		public void TestLoad1()
		{
			var assemblyFileName = "Foo1.dll";
			if (File.Exists(assemblyFileName))
				File.Delete(assemblyFileName);

			var builder = new PluginBuilder("Simon", "Foo1", "Foo1", "Simon", "None of your business", "Get of my lawn");
			builder.ImplementInterface<ILogEntryParserPlugin>("Foo1.MyAwesomePlugin");
			builder.Save();
			var description = new PluginDescription
			{
				FilePath = assemblyFileName,
				PluginImplementations = new []
				{
					new PluginImplementationDescription("Foo1.MyAwesomePlugin", typeof(ILogEntryParserPlugin))
				}
			};

			using (var scanner = new PluginAssemblyLoader())
			{
				var plugin = scanner.Load<ILogEntryParserPlugin>(description, description.PluginImplementations[0]);
				plugin.Should().NotBeNull();
				plugin.GetType().FullName.Should().Be("Foo1.MyAwesomePlugin");
			}
		}

		[Test]
		public void TestLoad2()
		{
			var assemblyFileName = "Foo2.dll";
			if (File.Exists(assemblyFileName))
				File.Delete(assemblyFileName);

			var builder = new PluginBuilder("Simon", "Foo2", "Foo2", "Simon", "None of your business", "Get of my lawn");
			builder.ImplementInterface<ILogEntryParserPlugin>("Foo2.MyAwesomePlugin");
			builder.Save();
			var description = new PluginDescription
			{
				FilePath = assemblyFileName,
				PluginImplementations = new List<IPluginImplementationDescription>
				{
					new PluginImplementationDescription("Foo2.MyAwesomePlugin", typeof(ILogEntryParserPlugin))
				}
			};

			using (var scanner = new PluginAssemblyLoader())
			{
				var plugin = scanner.Load<ILogEntryParserPlugin>(description, description.PluginImplementations[0]);
				plugin.Should().NotBeNull();
				plugin.GetType().FullName.Should().Be("Foo2.MyAwesomePlugin");
			}
		}

		[Test]
		[Description("Verifies that both PluginScanner and PluginLoader play nice together")]
		public void TestScanAndLoad()
		{
			using (var scanner = new PluginAssemblyLoader())
			{
				var assemblyFileName = "Foo3.dll";
				if (File.Exists(assemblyFileName))
					File.Delete(assemblyFileName);

				var builder = new PluginBuilder("Simon", "Foo3", "Foo3", "Simon", "None of your business", "Get of my lawn");
				builder.ImplementInterface<ILogEntryParserPlugin>("Foo3.MyAwesomePlugin");
				builder.Save();

				var description = scanner.ReflectPlugin(assemblyFileName);
				var plugin = scanner.Load<ILogEntryParserPlugin>(description, description.PluginImplementations[0]);
				plugin.Should().NotBeNull();
				plugin.GetType().FullName.Should().Be("Foo3.MyAwesomePlugin");
			}
		}

		[Test]
		[Description("Verifies that LoadAllOfType simply skips plugins that cannot be loaded")]
		public void TestLoadAllOfType1()
		{
			using (var scanner = new PluginAssemblyLoader())
			{
				var description = new PluginDescription
				{
					FilePath = "some nonexistant assembly",
					PluginImplementations = new List<IPluginImplementationDescription>
					{
						new PluginImplementationDescription("Foo1.MyAwesomePlugin", typeof(ILogEntryParserPlugin))
					}
				};

				new Action(() => scanner.LoadAllOfType<ILogEntryParserPlugin>()).Should().NotThrow();
				scanner.LoadAllOfType<ILogEntryParserPlugin>().Should().BeEmpty();
			}
		}
	}
}