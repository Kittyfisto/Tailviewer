using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using log4net;

namespace Tailviewer.Core
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class TypeFactory
		: ITypeFactory
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly Type[] NoTypes = new Type[0];
		private static readonly ParameterModifier[] NoModifiers = new ParameterModifier[0];

		private readonly IReadOnlyDictionary<string, Func<ISerializable>> _factories;

		/// <summary>
		/// Initializes this 
		/// </summary>
		/// <param name="types"></param>
		public TypeFactory(IEnumerable<KeyValuePair<string, Type>> types)
		{
			var factoriesByType = new Dictionary<Type, Func<ISerializable>>();
			var factories = new Dictionary<string, Func<ISerializable>>();

			foreach (var pair in types)
			{
				// Once plugins begin renaming their types its quite plausible to have the same
				// type identified with different names and thus we don't want to create the same
				// factory over an over, so we cache it...
				Func<ISerializable> factory;
				if (!factoriesByType.TryGetValue(pair.Value, out factory))
				{
					factory = CreateFactory(pair.Value);
					factoriesByType.Add(pair.Value, factory);
				}

				// In the end we only care about factories we can 
				if (factory != null)
				{
					factories.Add(pair.Key, factory);
				}
			}
			_factories = factories;
		}

		private Func<ISerializable> CreateFactory(Type type)
		{
			var ctor = type.GetConstructor(BindingFlags.CreateInstance | BindingFlags.Public, null, CallingConventions.Any, NoTypes, NoModifiers);
			if (ctor == null)
			{
				Log.ErrorFormat("Unable to find parameterless public constructor for: {0}", type.AssemblyQualifiedName);
				return null;
			}

			var expression = Expression.Lambda<Func<ISerializable>>(Expression.New(ctor));
			var factory = expression.Compile();
			return factory;
		}

		/// <inheritdoc />
		public ISerializable TryCreateNew(string typeName)
		{
			if (typeName == null)
				return null;

			Func<ISerializable> factory;
			if (!_factories.TryGetValue(typeName, out factory))
			{
				return null;
			}

			try
			{
				return factory();
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Creating new '{0}' threw an unexpected exception: {1}", typeName, e);
				return null;
			}
		}
	}
}