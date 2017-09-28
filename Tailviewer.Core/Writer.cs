using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using log4net;

namespace Tailviewer.Core
{
	/// <summary>
	/// </summary>
	public sealed class Writer
		: IWriter
		, IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly XmlWriter _writer;
		private bool _isDisposed;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="output"></param>
		public Writer(Stream output)
		{
			var settings = new XmlWriterSettings
			{
				Indent = true,
			};
			_writer = XmlWriter.Create(output, settings);
			_writer.WriteStartDocument();
			_writer.WriteStartElement("Document");
		}

		/// <inheritdoc />
		public void WriteAttribute(string name, string value)
		{
			_writer.WriteAttributeString(name, value);
		}

		/// <inheritdoc />
		public void WriteAttribute(string name, int value)
		{
			WriteAttribute(name, value.ToString(CultureInfo.InvariantCulture));
		}

		/// <inheritdoc />
		public void WriteAttribute(string name, ISerializable value)
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
		public void WriteAttribute(string name, IEnumerable<ISerializable> values)
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

		public void Flush()
		{
			_writer.Flush();
		}

		private void WriteCustomType(ISerializable value)
		{
			if (value != null)
			{
				var type = value.GetType();
				var typeName = type.FullName;
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