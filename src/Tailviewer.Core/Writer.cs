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
	/// Responsible for serializing an object graph in a stream.
	/// The resulting stream contains an xml document which can be read again using a <see cref="Reader"/>.
	/// </summary>
	public sealed class Writer
		: IWriter
		, IDisposable
	{
		private readonly ITypeFactory _typeFactory;

		/// <summary>
		///     The current version of the xml format, produced by this writer.
		///     Has to be incremented when backwards incompatible changes have been
		///     made.
		/// </summary>
		public const int FormatVersion = 1;

		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly DateTime _created;
		private readonly XmlWriter _writer;
		private bool _isDisposed;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="output"></param>
		/// <param name="typeFactory"></param>
		public Writer(Stream output, ITypeFactory typeFactory)
		{
			_typeFactory = typeFactory ?? throw new ArgumentNullException(nameof(typeFactory));
			_created = DateTime.Now;

			var settings = new XmlWriterSettings
			{
				Indent = true
			};
			_writer = XmlWriter.Create(output, settings);
			_writer.WriteStartDocument();
			_writer.WriteStartElement("Document");
			WriteAttribute("FormatVersion", FormatVersion.ToString(CultureInfo.InvariantCulture));
			WriteAttribute("Created", _created);
			WriteAttribute("TailviewerVersion", Constants.ApplicationVersion);
			WriteAttribute("TailviewerBuildDate", Constants.BuildDate);
		}

		/// <summary>
		///     The timestamp when this writer was created.
		/// </summary>
		public DateTime Created => _created;

		/// <inheritdoc />
		public void WriteAttribute(string name, bool value)
		{
			_writer.WriteStartElement(name);
			_writer.WriteValue(value);
			_writer.WriteEndElement();
		}

		/// <inheritdoc />
		public void WriteAttribute(string name, string value)
		{
			_writer.WriteStartElement(name);
			_writer.WriteValue(value);
			_writer.WriteEndElement();
		}

		/// <inheritdoc />
		public void WriteAttribute(string name, Version value)
		{
			WriteAttribute(name, value?.ToString() ?? string.Empty);
		}

		/// <inheritdoc />
		public void WriteAttribute(string name, DateTime value)
		{
			var tmp = value.ToString("o");
			WriteAttribute(name, tmp);
		}

		/// <inheritdoc />
		public void WriteAttribute(string name, Guid value)
		{
			WriteAttribute(name, value.ToString());
		}

		/// <inheritdoc />
		public void WriteAttribute(string name, int value)
		{
			WriteAttribute(name, value.ToString(CultureInfo.InvariantCulture));
		}

		/// <inheritdoc />
		public void WriteAttribute(string name, long value)
		{
			WriteAttribute(name, value.ToString(CultureInfo.InvariantCulture));
		}

		/// <inheritdoc />
		public void WriteAttribute(string name, WidgetId value)
		{
			WriteAttribute(name, value.ToString());
		}

		/// <inheritdoc />
		public void WriteAttribute(string name, AnalyserPluginId value)
		{
			WriteAttribute(name, value.ToString());
		}

		/// <inheritdoc />
		public void WriteAttribute(string name, DataSourceId value)
		{
			WriteAttribute(name, value.ToString());
		}

		/// <inheritdoc />
		public void WriteAttribute(string name, AnalysisId value)
		{
			WriteAttribute(name, value.ToString());
		}

		/// <inheritdoc />
		public void WriteAttribute(string name, AnalyserId value)
		{
			WriteAttribute(name, value.ToString());
		}

		/// <inheritdoc />
		public void WriteAttribute(string name, ISerializableType value)
		{
			_writer.WriteStartElement(name);
			try
			{
				WriteCustomType(value);
			}
			finally
			{
				_writer.WriteEndElement();
			}
		}

		/// <inheritdoc />
		public void WriteAttribute(string name, IEnumerable<ISerializableType> values)
		{
			_writer.WriteStartElement(name);
			try
			{
				var tmp = values.ToList();
				WriteAttribute("Count", tmp.Count);
				foreach (var value in tmp)
				{
					_writer.WriteStartElement("Element");
					try
					{
						WriteCustomType(value);
					}
					finally
					{
						_writer.WriteEndElement();
					}
				}
			}
			finally
			{
				_writer.WriteEndElement();
			}
		}

		/// <inheritdoc />
		public void WriteAttributeEnum<T>(string name, T value)
		{
			WriteAttribute(name, value.ToString());
		}

		private void WriteCustomType(ISerializableType value)
		{
			if (value != null)
			{
				var type = value.GetType();
				var typeName = _typeFactory.TryGetTypeName(type);
				if (typeName != null)
				{
					WriteAttribute("Type", typeName);
					_writer.WriteStartElement("Value");
					try
					{
						value.Serialize(this);
					}
					catch (Exception e)
					{
						Log.ErrorFormat("Caught unexpected exception while trying to serialize '{0}': {1}", value, e);
					}
					finally
					{
						_writer.WriteEndElement();
					}
				}
				else
				{
					Log.ErrorFormat("Unable to serialize type '{0}': It has not been registered with the type factory", type);
				}
			}
		}

		/// <inheritdoc />
		public void Dispose()
		{
			if (!_isDisposed)
			{
				_writer.WriteEndElement();
				_writer.WriteEndDocument();
				_writer?.Dispose();
				_isDisposed = true;
			}
		}
	}
}