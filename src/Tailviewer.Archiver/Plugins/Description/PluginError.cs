namespace Tailviewer.Archiver.Plugins.Description
{
	/// <summary>
	///    Contains a human readable error about a plugin.
	/// </summary>
	public sealed class PluginError
	{
		public string What { get; set; }

		public PluginError(string what)
		{
			What = what;
		}

		#region Equality members

		private bool Equals(PluginError other)
		{
			return string.Equals(What, other.What);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is PluginError && Equals((PluginError) obj);
		}

		public override int GetHashCode()
		{
			return (What != null ? What.GetHashCode() : 0);
		}

		public static bool operator ==(PluginError left, PluginError right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(PluginError left, PluginError right)
		{
			return !Equals(left, right);
		}

		#endregion
	}
}