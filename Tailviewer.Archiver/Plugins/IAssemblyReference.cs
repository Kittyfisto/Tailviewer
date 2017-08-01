namespace Tailviewer.Archiver.Plugins
{
	/// <summary>
	/// 
	/// </summary>
	public interface IAssemblyReference
	{
		/// <summary>
		/// The fully qualified assembly name in the form "UtilityLibrary, Version=1.1.0.0, Culture=neutral, PublicKeyToken=null"
		/// </summary>
		string FullName { get; set; }
	}
}