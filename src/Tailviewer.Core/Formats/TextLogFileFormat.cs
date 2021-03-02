using System.Text;
using Tailviewer.Api;

// ReSharper disable once CheckNamespace
namespace Tailviewer.Core
{
	internal sealed class TextLogFileFormat
		: ILogFileFormat
	{
		public TextLogFileFormat(string name,
		                         string description = null,
		                         Encoding encoding = null)
		{
			Name = name;
			Description = description;
			Encoding = encoding;
		}

		#region Implementation of ILogFileFormat

		public string Name { get; }

		public string Description { get; }

		public bool IsText => true;

		public Encoding Encoding { get; }

		#endregion

		#region Overrides of Object

		public override string ToString()
		{
			return Name;
		}

		#endregion
	}
}