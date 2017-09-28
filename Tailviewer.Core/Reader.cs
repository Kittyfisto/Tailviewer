using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace Tailviewer.Core
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class Reader
		: IReader
	{
		private readonly ITypeFactory _typeFactory;
		private readonly XmlReader _reader;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="input"></param>
		/// <param name="typeFactory"></param>
		public Reader(Stream input, ITypeFactory typeFactory)
		{
			_typeFactory = typeFactory;
			_reader = XmlReader.Create(input);
			_reader.MoveToContent();
		}

		/// <inheritdoc />
		public bool TryReadAttribute(string name, out string value)
		{
			if (!_reader.MoveToAttribute(name))
			{
				value = null;
				return false;
			}

			value = _reader.Value;
			return true;
		}

		/// <inheritdoc />
		public bool TryReadAttribute(string name, out int value)
		{
			string tmp;
			if (!TryReadAttribute(name, out tmp))
			{
				value = default(int);
				return false;
			}

			if (!int.TryParse(tmp, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
			{
				return false;
			}

			return true;
		}

		/// <inheritdoc />
		public bool TryReadAttribute(string name, out ISerializable value)
		{
			throw new System.NotImplementedException();
		}

		/// <inheritdoc />
		public bool TryReadAttribute(string name, out IEnumerable<ISerializable> values)
		{
			throw new System.NotImplementedException();
		}
	}
}