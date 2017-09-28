using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using log4net;

namespace Tailviewer.Core
{
	/// <summary>
	/// </summary>
	public sealed class Reader
		: IReader
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ElementReader _documentReader;

		/// <summary>
		///     Initializes this reader.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="typeFactory"></param>
		public Reader(Stream input, ITypeFactory typeFactory)
		{
			var document = new XmlDocument();
			document.Load(input);

			_documentReader = new ElementReader(document.DocumentElement, typeFactory);
		}

		/// <inheritdoc />
		public bool TryReadAttribute(string name, out string value)
		{
			return _documentReader.TryReadAttribute(name, out value);
		}

		/// <inheritdoc />
		public bool TryReadAttribute(string name, out int value)
		{
			return _documentReader.TryReadAttribute(name, out value);
		}

		/// <inheritdoc />
		public bool TryReadAttribute(string name, out ISerializable value)
		{
			return _documentReader.TryReadAttribute(name, out value);
		}

		/// <inheritdoc />
		public bool TryReadAttribute<T>(string name, out T value) where T : class, ISerializable
		{
			return _documentReader.TryReadAttribute(name, out value);
		}

		public bool TryReadAttribute<T>(string name, out T? value) where T : struct, ISerializable
		{
			return _documentReader.TryReadAttribute(name, out value);
		}

		/// <inheritdoc />
		public bool TryReadAttribute(string name, out IEnumerable<ISerializable> values)
		{
			return _documentReader.TryReadAttribute(name, out values);
		}

		private sealed class ElementReader
			: IReader
		{
			private readonly Dictionary<string, XmlElement> _childElements;
			private readonly XmlElement _element;
			private readonly ITypeFactory _typeFactory;

			public ElementReader(XmlElement element, ITypeFactory typeFactory)
			{
				_element = element;
				_typeFactory = typeFactory;

				var childNodes = element.ChildNodes;
				_childElements = new Dictionary<string, XmlElement>(childNodes.Count);
				foreach (XmlElement child in childNodes)
					if (child != null)
						_childElements.Add(child.Name, child);
			}

			public bool TryReadAttribute(string name, out string value)
			{
				var node = _element.GetAttributeNode(name);
				if (node == null)
				{
					value = null;
					return false;
				}

				value = node.Value;
				return true;
			}

			public bool TryReadAttribute(string name, out int value)
			{
				string tmp;
				if (!TryReadAttribute(name, out tmp))
				{
					value = default(int);
					return false;
				}

				if (!int.TryParse(tmp, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
					return false;

				return true;
			}

			public bool TryReadAttribute(string name, out ISerializable value)
			{
				XmlElement element;
				if (!_childElements.TryGetValue(name, out element))
				{
					value = null;
					return false;
				}

				return TryReadChild(element, out value);
			}

			public bool TryReadAttribute<T>(string name, out T value) where T : class, ISerializable
			{
				ISerializable tmp;
				if (!TryReadAttribute(name, out tmp))
				{
					value = default(T);
					return false;
				}

				if (tmp == null)
				{
					value = null;
					return true;
				}

				value = tmp as T;
				return value != null;
			}

			public bool TryReadAttribute<T>(string name, out T? value) where T : struct, ISerializable
			{
				ISerializable tmp;
				if (!TryReadAttribute(name, out tmp))
				{
					value = default(T);
					return false;
				}

				if (tmp == null)
				{
					value = null;
					return true;
				}

				value = tmp as T?;
				return value != null;
			}

			public bool TryReadAttribute(string name, out IEnumerable<ISerializable> values)
			{
				XmlElement element;
				if (!_childElements.TryGetValue(name, out element))
				{
					values = null;
					return false;
				}

				var node = element.GetAttributeNode("Count");
				if (node == null)
				{
					values = null;
					return false;
				}

				int count;
				if (!int.TryParse(node.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out count))
				{
					values = null;
					return false;
				}

				var tmp = new List<ISerializable>(count);

				foreach (XmlElement child in element.ChildNodes)
				{
					if (child != null && child.Name == "Element")
					{
						ISerializable value;
						if (TryReadChild(child, out value))
						{
							tmp.Add(value);
						}
					}
				}

				values = tmp;
				return true;
			}

			private bool TryReadChild(XmlElement element, out ISerializable value)
			{
				var node = element.GetAttributeNode("Type");
				if (node == null) //< This is the case when a null value was persisted
				{
					value = null;
					return true;
				}

				var typeName = node.Value;
				value = _typeFactory.TryCreateNew(typeName);
				if (value == null) //< Type doesn't exist or couldn't be created
					return false;

				var child = element.FirstChild as XmlElement;
				if (child == null || child.Name != "Value")
					return false;

				var reader = new ElementReader(child, _typeFactory);
				try
				{
					value.Deserialize(reader);
					return true;
				}
				catch (Exception e)
				{
					Log.ErrorFormat("Caught unexpected exception while deserializing '{0}': {1}",
						typeName, e);
					return false;
				}
			}
		}
	}
}