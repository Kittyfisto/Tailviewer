using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using FluentAssertions;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.Archiver.Test
{
	public abstract class AbstractPluginTest
	{
		public sealed class PluginBuilder
		{
			private readonly AssemblyBuilder _assembly;
			private readonly ModuleBuilder _module;
			private readonly string _pluginName;
			private readonly string _fileName;

			public PluginBuilder(
				string pluginIdNamespace,
				string pluginIdName,
				string pluginName,
				string author = null,
				string website = null,
				string description = null)
			{
				_pluginName = pluginName;
				_fileName = string.Format("{0}.dll", pluginName);
				var assemblyName = new AssemblyName(pluginName);
				var attributes = new List<CustomAttributeBuilder>();
				if (pluginIdNamespace != null && pluginIdName != null)
					attributes.Add(CreateAttribute<PluginIdAttribute>(pluginIdNamespace, pluginIdName));
				if (author != null)
					attributes.Add(CreateAttribute<PluginAuthorAttribute>(author));
				if (website != null)
					attributes.Add(CreateAttribute<PluginWebsiteAttribute>(website));
				if (website != null)
					attributes.Add(CreateAttribute<PluginDescriptionAttribute>(description));
				_assembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Save, attributes);
				_module = _assembly.DefineDynamicModule(pluginName, _fileName);
				
			}

			public void ImplementInterface<T>(string fullName) where T : IPlugin
			{
				var interfaceType = typeof(T);
				var typeBuilder = _module.DefineType(fullName, TypeAttributes.Public | TypeAttributes.Sealed);
				typeBuilder.AddInterfaceImplementation(interfaceType);

				var properties = interfaceType.GetProperties();
				foreach (var property in properties)
				{
					var indexTypes = property.GetIndexParameters().Select(x => x.ParameterType).ToArray();
					var propertyBuilder = typeBuilder.DefineProperty(property.Name, property.Attributes,
						property.GetAccessors()[0].CallingConvention,
						property.PropertyType,
						indexTypes);

					var method = DefineMethod(typeBuilder,
						MethodAttributes.Public | MethodAttributes.Virtual,
						string.Format("get_{0}", property.Name),
						property.GetMethod.CallingConvention,
						property.PropertyType,
						indexTypes);
					propertyBuilder.SetGetMethod(method);
					typeBuilder.DefineMethodOverride(method, property.GetMethod);
				}

				var methods = interfaceType.GetMethods();
				foreach (var interfaceMethod in methods)
				{
					if (!interfaceMethod.IsSpecialName)
					{
						var method = DefineMethod(typeBuilder,
							MethodAttributes.Public | MethodAttributes.Virtual,
							interfaceMethod.Name,
							interfaceMethod.CallingConvention,
							interfaceMethod.ReturnType,
							interfaceMethod.GetParameters().Select(x => x.ParameterType).ToArray());
						typeBuilder.DefineMethodOverride(method, interfaceMethod);
					}
				}
				typeBuilder.CreateType();
			}

			private MethodBuilder DefineMethod(TypeBuilder typeBuilder, MethodAttributes attributes, string name, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
			{
				var methodBuilder = typeBuilder.DefineMethod(name, attributes,
					callingConvention,
					returnType,
					null,
					null,
					parameterTypes,
					null,
					null);
				if (returnType != typeof(void))
				{
					var gen = methodBuilder.GetILGenerator();
					if (returnType.IsValueType)
					{
						gen.Emit(OpCodes.Ldc_I4_0);
					}
					else
					{
						gen.Emit(OpCodes.Ldnull);
					}
					gen.Emit(OpCodes.Ret);
				}
				return methodBuilder;
			}

			public void Save()
			{
				_assembly.Save(_fileName);
			}

			public void Save(string fileName)
			{
				_assembly.Save(fileName);
			}

			public string FileName => _fileName;

			public string AssemblyFileVersion
			{
				set
				{
					var ctor = typeof(AssemblyFileVersionAttribute).GetConstructor(new[] {typeof(string)});
					var builder = new CustomAttributeBuilder(ctor, new object[] {value});
					_assembly.SetCustomAttribute(builder);
				}
			}

			public string AssemblyVersion
			{
				set
				{
					var ctor = typeof(AssemblyVersionAttribute).GetConstructor(new[] { typeof(string) });
					var builder = new CustomAttributeBuilder(ctor, new object[] { value });
					_assembly.SetCustomAttribute(builder);
				}
			}

			public string AssemblyInformationalVersion
			{
				set
				{
					var ctor = typeof(AssemblyInformationalVersionAttribute).GetConstructor(new[] { typeof(string) });
					var builder = new CustomAttributeBuilder(ctor, new object[] { value });
					_assembly.SetCustomAttribute(builder);
				}
			}

			public Version PluginVersion
			{
				set
				{
					if (value.Revision == -1)
					{
						if (value.Build == -1)
						{
							var ctor = typeof(PluginVersionAttribute).GetConstructor(new[]{typeof(int), typeof(int)});
							var builder = new CustomAttributeBuilder(ctor,
								new object[] {value.Major, value.Minor});
							_assembly.SetCustomAttribute(builder);
						}
						else
						{
							var ctor = typeof(PluginVersionAttribute).GetConstructor(new[] { typeof(int), typeof(int), typeof(int) });
							var builder = new CustomAttributeBuilder(ctor, new object[] { value.Major, value.Minor, value.Build });
							_assembly.SetCustomAttribute(builder);
						}
					}
					else
					{
						var ctor = typeof(PluginVersionAttribute).GetConstructor(new[]
							{typeof(int), typeof(int), typeof(int), typeof(int)});
						var builder = new CustomAttributeBuilder(ctor,
							new object[] {value.Major, value.Minor, value.Build, value.Revision});
						_assembly.SetCustomAttribute(builder);
					}
				}
			}

			private static CustomAttributeBuilder CreateAttribute<T>(params object[] parameters) where T : Attribute
			{
				var type = typeof(T);
				var ctors = type.GetConstructors();
				var builder = new CustomAttributeBuilder(ctors.First(), parameters);
				return builder;
			}
		}

		protected static void CreatePlugin(string assemblyFileName,
			string author = null,
			string website = null,
			string description = null)
		{
			var pluginName = Path.GetFileNameWithoutExtension(assemblyFileName);
			var idParts = pluginName.Split('.');
			idParts.Should().HaveCount(2);
			var @namespace = idParts[0];
			var name = idParts[1];

			var builder = new PluginBuilder(@namespace, name, pluginName, author, website, description);
			builder.Save();
		}
	}
}