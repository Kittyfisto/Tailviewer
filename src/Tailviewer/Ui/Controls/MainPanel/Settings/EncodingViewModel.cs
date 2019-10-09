using System.Text;

namespace Tailviewer.Ui.Controls.MainPanel.Settings
{
	public sealed class EncodingViewModel
	{
		public readonly Encoding Encoding;
		private readonly string _name;

		public EncodingViewModel(Encoding encoding)
		{
			Encoding = encoding;
		}

		public EncodingViewModel(Encoding encoding, string name)
		{
			Encoding = encoding;
			_name = name;
		}

		#region Equality members

		private bool Equals(EncodingViewModel other)
		{
			return Equals(Encoding, other.Encoding);
		}

		public override bool Equals(object obj)
		{
			return ReferenceEquals(this, obj) || obj is EncodingViewModel other && Equals(other);
		}

		public override int GetHashCode()
		{
			return (Encoding != null ? Encoding.GetHashCode() : 0);
		}

		#endregion

		#region Overrides of Object

		public override string ToString()
		{
			return _name ?? Encoding?.WebName ?? string.Empty;
		}

		#endregion
	}
}