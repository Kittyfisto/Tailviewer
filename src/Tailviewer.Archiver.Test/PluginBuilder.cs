using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.Archiver.Test
{
	public sealed class PluginBuilder
	{
		private readonly AssemblyBuilder _assembly;
		private readonly ModuleBuilder _module;
		private readonly string _pluginName;
		private readonly string _fileName;
		private TypeBuilder _referencer;
		private readonly List<string> _dependencies;
		private readonly List<TypeBuilder> _types;

		public PluginBuilder(
			string pluginIdNamespace,
			string pluginIdName,
			string pluginName,
			string author = null,
			string website = null,
			string description = null,
			Version version = null)
		{
			_pluginName = pluginName;
			_fileName = $"{pluginName}.dll";
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
			if (version != null)
			{
				if (version.Build == -1)
					attributes.Add(CreateAttribute<PluginVersionAttribute>(version.Major, version.Minor));
				else if (version.Revision == -1)
					attributes.Add(CreateAttribute<PluginVersionAttribute>(version.Major, version.Minor, version.Build));
				else
					attributes.Add(CreateAttribute<PluginVersionAttribute>(version.Major, version.Minor, version.Build, version.Revision));
			}
			_assembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Save, attributes);
			_module = _assembly.DefineDynamicModule(pluginName, _fileName);

			_dependencies = new List<string>();
			_types = new List<TypeBuilder>();
			_referencer = _module.DefineType("HoldsReferences", TypeAttributes.Class | TypeAttributes.Public);
		}

		public void AddDependency(string filePath)
		{
			var assembly = Assembly.LoadFile(filePath);
			var name = $"Dependency{_dependencies.Count}";
			_referencer.DefineField(name, assembly.GetExportedTypes().First(),
				FieldAttributes.Static | FieldAttributes.Public);
			_dependencies.Add(filePath);
		}

		public TypeBuilder ImplementInterface<T>(string fullName) where T : IPlugin
		{
			var interfaceType = typeof(T);
			var typeBuilder = _module.DefineType(fullName, TypeAttributes.Public | TypeAttributes.Sealed);
			_types.Add(typeBuilder);
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
					String.Format("get_{0}", property.Name),
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

			return typeBuilder;
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
			FinishBuild();
			_assembly.Save(_fileName);
		}

		public void Save(string fileName)
		{
			FinishBuild();
			_assembly.Save(fileName);
		}

		private void FinishBuild()
		{
			_referencer.CreateType();
			foreach(var typeBuilder in _types)
				typeBuilder.CreateType();
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

		private static CustomAttributeBuilder CreateAttribute<T>(params object[] arguments) where T : Attribute
		{
			var type = typeof(T);
			var ctors = type.GetConstructors();
			var ctor = ctors.First(x => x.GetParameters().Length == arguments.Length);
			var builder = new CustomAttributeBuilder(ctor, arguments);
			return builder;
		}

		public TypeBuilder DefineType(string name, TypeAttributes attr)
		{
			return _module.DefineType(name, attr);
		}

		public CustomAttributeBuilder BuildCustomAttribute(Attribute attribute)
		{
			Type type = attribute.GetType();
			var constructor = type.GetConstructor(Type.EmptyTypes);
			var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.CanRead && x.CanWrite).ToArray();
			var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance).Where(x => !x.IsInitOnly).ToArray();

			var propertyValues = properties.Select(x => x.GetValue(attribute, null)).ToList();
			var fieldValues = fields.Select(x => x.GetValue(attribute)).ToList();

			return new CustomAttributeBuilder(constructor, 
			                                  Type.EmptyTypes,
			                                  properties,
			                                  propertyValues.ToArray(),
			                                  fields,
			                                  fieldValues.ToArray());
		}
	}
}