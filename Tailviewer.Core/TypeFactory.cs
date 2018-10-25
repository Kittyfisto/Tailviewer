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

		private readonly Dictionary<Type, Func<ISerializableType>> _factoriesByType;
		private readonly Dictionary<string, Func<ISerializableType>> _factoriesByName;
		private readonly object _syncRoot;

		/// <summary>
		/// Creates an empty factory which cannot create any custom type.
		/// Call Add repeatedly to be able to construct custom types.
		/// </summary>
		public TypeFactory()
		{
			_syncRoot = new object();
			_factoriesByType = new Dictionary<Type, Func<ISerializableType>>();
			_factoriesByName = new Dictionary<string, Func<ISerializableType>>();
		}

		/// <summary>
		/// Initializes this 
		/// </summary>
		/// <param name="types"></param>
		public TypeFactory(IEnumerable<KeyValuePair<string, Type>> types)
			: this()
		{
			foreach (var pair in types)
			{
				Add(pair.Key, pair.Value);
			}
		}

		/// <summary>
		/// Registers the given type under its <see cref="Type.FullName"/>.
		/// </summary>
		/// <param name="type"></param>
		public void Add(Type type)
		{
			Add(type.FullName, type);
		}

		/// <summary>
		/// Registers the given type under the given name.
		/// From now on, calls to <see cref="TryCreateNew"/> when called with the given name will
		/// call the given type's default ctor and return true if the type could be created (or false if it threw an exception).
		/// </summary>
		/// <param name="name"></param>
		/// <param name="type"></param>
		public void Add(string name, Type type)
		{
			lock (_syncRoot)
			{
				// Once plugins begin renaming their types its quite plausible to have the same
				// type identified with different names and thus we don't want to create the same
				// factory over an over, so we cache it...
				if (!_factoriesByType.TryGetValue(type, out var factory))
				{
					factory = CreateFactory(type);
					_factoriesByType.Add(type, factory);
				}

				if (!_factoriesByName.ContainsKey(name))
				{
					_factoriesByName.Add(name, factory);
				}
				else
				{
					Log.WarnFormat("Unable to register factory for type '{0}', there already exists one!", name);
				}
			}
		}

		private Func<ISerializableType> CreateFactory(Type type)
		{
			var ctor = type.GetConstructor(NoTypes);
			if (ctor == null)
			{
				Log.ErrorFormat("Unable to find parameterless public constructor for: {0}", type.AssemblyQualifiedName);
				return null;
			}

			var expression = Expression.Lambda<Func<ISerializableType>>(Expression.New(ctor));
			var factory = expression.Compile();
			return factory;
		}

		/// <inheritdoc />
		public ISerializableType TryCreateNew(string typeName)
		{
			if (typeName == null)
				return null;

			if (!_factoriesByName.TryGetValue(typeName, out var factory))
			{
				Log.WarnFormat("Type '{0}' is unknown and cannot be created!", typeName);
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