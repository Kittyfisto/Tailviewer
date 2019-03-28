using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using log4net;
using Tailviewer.BusinessLogic.Analysis;

namespace Tailviewer.Core
{
	/// <summary>
	/// Responsible for reading an object graph from a stream which has previously been persisted using
	/// a <see cref="Writer"/>.
	/// </summary>
	public sealed class Reader
		: IReader
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private readonly DateTime _created;

		private readonly ElementReader _documentReader;
		private readonly int _formatVersion;
		private readonly DateTime _tailviewerBuildDate;
		private readonly Version _tailviewerVersion;

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
			_documentReader.TryReadAttribute("FormatVersion", out _formatVersion);
			_documentReader.TryReadAttribute("Created", out _created);
			_documentReader.TryReadAttribute("TailviewerVersion", out _tailviewerVersion);
			_documentReader.TryReadAttribute("TailviewerBuildDate", out _tailviewerBuildDate);
		}

		/// <summary>
		///     The timestamp when the document was created.
		/// </summary>
		public DateTime Created => _created;

		/// <summary>
		///     The format version the document was created with.
		/// </summary>
		public int FormatVersion => _formatVersion;

		/// <summary>
		///     The version of the application that created the document.
		/// </summary>
		public Version TailviewerVersion => _tailviewerVersion;

		/// <summary>
		///     The build date of the application that created the document.
		/// </summary>
		public DateTime TailviewerBuildDate => _tailviewerBuildDate;

		/// <inheritdoc />
		public bool TryReadAttribute(string name, out bool value)
		{
			return _documentReader.TryReadAttribute(name, out value);
		}

		/// <inheritdoc />
		public bool TryReadAttribute(string name, out string value)
		{
			return _documentReader.TryReadAttribute(name, out value);
		}

		/// <inheritdoc />
		public bool TryReadAttribute(string name, out Version value)
		{
			return _documentReader.TryReadAttribute(name, out value);
		}

		/// <inheritdoc />
		public bool TryReadAttribute(string name, out DateTime value)
		{
			return _documentReader.TryReadAttribute(name, out value);
		}

		/// <inheritdoc />
		public bool TryReadAttribute(string name, out DateTime? value)
		{
			return _documentReader.TryReadAttribute(name, out value);
		}

		/// <inheritdoc />
		public bool TryReadAttribute(string name, out Guid value)
		{
			return _documentReader.TryReadAttribute(name, out value);
		}

		/// <inheritdoc />
		public bool TryReadAttribute(string name, out int value)
		{
			return _documentReader.TryReadAttribute(name, out value);
		}

		/// <inheritdoc />
		public bool TryReadAttribute(string name, out long value)
		{
			return _documentReader.TryReadAttribute(name, out value);
		}

		/// <inheritdoc />
		public bool TryReadAttribute(string name, out long? value)
		{
			return _documentReader.TryReadAttribute(name, out value);
		}

		/// <inheritdoc />
		public bool TryReadAttribute(string name, out WidgetId value)
		{
			return _documentReader.TryReadAttribute(name, out value);
		}

		/// <inheritdoc />
		public bool TryReadAttribute(string name, out AnalyserPluginId value)
		{
			return _documentReader.TryReadAttribute(name, out value);
		}

		/// <inheritdoc />
		public bool TryReadAttribute(string name, out DataSourceId value)
		{
			return _documentReader.TryReadAttribute(name, out value);
		}

		/// <inheritdoc />
		public bool TryReadAttribute(string name, out AnalysisId value)
		{
			return _documentReader.TryReadAttribute(name, out value);
		}

		/// <inheritdoc />
		public bool TryReadAttribute(string name, out AnalyserId value)
		{
			return _documentReader.TryReadAttribute(name, out value);
		}

		/// <inheritdoc />
		public bool TryReadAttribute(string name, out ISerializableType value)
		{
			return _documentReader.TryReadAttribute(name, out value);
		}

		/// <inheritdoc />
		public bool TryReadAttribute<T>(string name, out T value) where T : ISerializableType
		{
			return _documentReader.TryReadAttribute(name, out value);
		}

		/// <inheritdoc />
		public bool TryReadAttribute<T>(string name, T value) where T : class, ISerializableType
		{
			return _documentReader.TryReadAttribute(name, value);
		}

		/// <inheritdoc />
		public bool TryReadAttribute(string name, out IEnumerable<ISerializableType> values)
		{
			return _documentReader.TryReadAttribute(name, out values);
		}

		/// <inheritdoc />
		public bool TryReadAttribute<T>(string name, out IEnumerable<T> values) where T : class, ISerializableType
		{
			return _documentReader.TryReadAttribute(name, out values);
		}

		/// <inheritdoc />
		public bool TryReadAttribute<T>(string name, List<T> values) where T : class, ISerializableType
		{
			return _documentReader.TryReadAttribute(name, values);
		}

		/// <inheritdoc />
		public bool TryReadAttributeEnum<T>(string name, out T value) where T : struct
		{
			return _documentReader.TryReadAttributeEnum(name, out value);
		}

		private sealed class ElementReader
			: IReader
		{
			private readonly Dictionary<string, XmlElement> _childElements;
			private readonly ITypeFactory _typeFactory;

			public ElementReader(XmlElement element, ITypeFactory typeFactory)
			{
				_typeFactory = typeFactory;

				var childNodes = element.ChildNodes;
				_childElements = new Dictionary<string, XmlElement>(childNodes.Count);
				foreach (XmlElement child in childNodes)
					if (child != null)
						_childElements.Add(child.Name, child);
			}

			public bool TryReadAttribute(string name, out bool value)
			{
				if (TryReadAttribute(name, out string stringValue) && bool.TryParse(stringValue, out value))
					return true;

				value = false;
				return false;
			}

			public bool TryReadAttribute(string name, out string value)
			{
				XmlElement element;
				if (!_childElements.TryGetValue(name, out element))
				{
					value = null;
					return false;
				}

				value = element.InnerText;
				return true;
			}

			public bool TryReadAttribute(string name, out Version value)
			{
				string tmp;
				if (!TryReadAttribute(name, out tmp))
				{
					value = null;
					return false;
				}

				if (string.IsNullOrEmpty(tmp)) //< writer.Write(..., null)
				{
					value = null;
					return true;
				}

				if (!Version.TryParse(tmp, out value))
					return false;

				return true;
			}

			public bool TryReadAttribute(string name, out DateTime value)
			{
				string stringValue;
				if (!TryReadAttribute(name, out stringValue))
				{
					value = default(DateTime);
					return false;
				}

				return TryParseDateTime(stringValue, out value);
			}

			public bool TryReadAttribute(string name, out DateTime? value)
			{
				if (!TryReadAttribute(name, out string stringValue))
				{
					value = null;
					return false;
				}

				if (!TryParseDateTime(stringValue, out var actualValue))
				{
					value = null;
					return false;
				}

				value = actualValue;
				return true;
			}

			public bool TryReadAttribute(string name, out Guid value)
			{
				string tmp;
				if (!TryReadAttribute(name, out tmp))
				{
					value = default(Guid);
					return false;
				}

				value = Guid.Parse(tmp);
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

			public bool TryReadAttribute(string name, out long value)
			{
				string tmp;
				if (!TryReadAttribute(name, out tmp))
				{
					value = default(int);
					return false;
				}

				return TryParseLong(tmp, out value);
			}

			public bool TryReadAttribute(string name, out long? value)
			{
				string tmp;
				if (!TryReadAttribute(name, out tmp))
				{
					value = null;
					return false;
				}

				if (!TryParseLong(tmp, out var actualValue))
				{
					value = null;
					return false;
				}

				value = actualValue;
				return true;
			}

			public bool TryReadAttribute(string name, out WidgetId value)
			{
				Guid tmp;
				if (!TryReadAttribute(name, out tmp))
				{
					value = default(WidgetId);
					return false;
				}

				value = new WidgetId(tmp);
				return true;
			}

			public bool TryReadAttribute(string name, out AnalyserPluginId value)
			{
				string tmp;
				if (!TryReadAttribute(name, out tmp))
				{
					value = default(AnalyserPluginId);
					return false;
				}

				value = new AnalyserPluginId(tmp);
				return true;
			}

			public bool TryReadAttribute(string name, out DataSourceId value)
			{
				Guid tmp;
				if (!TryReadAttribute(name, out tmp))
				{
					value = default(DataSourceId);
					return false;
				}

				value = new DataSourceId(tmp);
				return true;
			}

			public bool TryReadAttribute(string name, out AnalysisId value)
			{
				Guid tmp;
				if (!TryReadAttribute(name, out tmp))
				{
					value = default(AnalysisId);
					return false;
				}

				value = new AnalysisId(tmp);
				return true;
			}

			public bool TryReadAttribute(string name, out AnalyserId value)
			{
				Guid tmp;
				if (!TryReadAttribute(name, out tmp))
				{
					value = default(AnalyserId);
					return false;
				}

				value = new AnalyserId(tmp);
				return true;
			}

			public bool TryReadAttribute(string name, out ISerializableType value)
			{
				return TryReadAttribute<ISerializableType>(name, out value);
			}

			public bool TryReadAttribute<T>(string name, out T value) where T : ISerializableType
			{
				XmlElement element;
				if (!_childElements.TryGetValue(name, out element))
				{
					value = default(T);
					return false;
				}

				return TryReadChild(element, out value);
			}

			public bool TryReadAttribute<T>(string name, T value) where T : class, ISerializableType
			{
				if (value == null)
					throw new ArgumentNullException(nameof(value), "You must construct the object beforehand if you use this overload. If you cannot do that, then you need to use the overload with value as an out parameter!");

				XmlElement element;
				if (!_childElements.TryGetValue(name, out element))
					return false;

				var child = element.ChildNodes.OfType<XmlElement>().FirstOrDefault(x => x.Name == "Value");
				if (child == null)
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
						name, e);
					return false;
				}
			}

			public bool TryReadAttribute(string name, out IEnumerable<ISerializableType> values)
			{
				return TryReadAttribute<ISerializableType>(name, out values);
			}

			public bool TryReadAttribute<T>(string name, out IEnumerable<T> values) where T : class, ISerializableType
			{
				var tmp = new List<T>();
				if (!TryReadAttribute(name, tmp))
				{
					values = null;
					return false;
				}

				values = tmp;
				return true;
			}

			public bool TryReadAttribute<T>(string name, List<T> values) where T : class, ISerializableType
			{
				XmlElement element;
				if (!_childElements.TryGetValue(name, out element))
					return false;

				var node = element.ChildNodes.OfType<XmlElement>().FirstOrDefault(x => x.Name == "Count");
				if (node == null)
					return false;

				int count;
				if (!int.TryParse(node.InnerText, NumberStyles.Integer, CultureInfo.InvariantCulture, out count))
					return false;

				values.Clear();
				TryReadChildren(element, values);
				return true;
			}

			public bool TryReadAttributeEnum<T>(string name, out T value) where T : struct
			{
				if (TryReadAttribute(name, out string stringValue) && Enum.TryParse(stringValue, out T tmp))
				{
					value = tmp;
					return true;
				}

				value = default(T);
				return false;
			}

			private static bool TryParseDateTime(string stringValue, out DateTime value)
			{
				if (!DateTime.TryParseExact(stringValue, "o", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out value))
					return false;

				return true;
			}

			private static bool TryParseLong(string stringValue, out long value)
			{
				if (!long.TryParse(stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
					return false;

				return true;
			}

			private bool TryReadChild<T>(XmlElement element, out T value) where T : ISerializableType
			{
				var type = element.GetElementsByTagName("Type").OfType<XmlElement>().FirstOrDefault();
				if (type == null) //< This is the case when a null value was persisted
				{
					value = default(T);
					return true;
				}

				var typeName = type.InnerText;
				var tmp = _typeFactory.TryCreateNew(typeName);
				if (!(tmp is T)) //< Type doesn't exist or couldn't be created
				{
					value = default(T);
					return false;
				}

				var child = element.GetElementsByTagName("Value").OfType<XmlElement>().FirstOrDefault();
				if (child == null)
				{
					value = default(T);
					return false;
				}

				var reader = new ElementReader(child, _typeFactory);
				try
				{
					value = (T) tmp;
					value.Deserialize(reader);
					return true;
				}
				catch (Exception e)
				{
					Log.ErrorFormat("Caught unexpected exception while deserializing '{0}': {1}",
						typeName, e);

					value = default(T);
					return false;
				}
			}

			private void TryReadChildren<T>(XmlElement element, List<T> values) where T : class, ISerializableType
			{
				foreach (XmlElement child in element.ChildNodes)
					if (child != null && child.Name == "Element")
					{
						T value;
						if (TryReadChild(child, out value))
							values.Add(value);
					}
			}
		}
	}
}