using System.Windows;
using System.Xml;

namespace Tailviewer.Settings
{
	public interface IMainWindowSettings
	{
		/// <summary>
		///     The height of the window.
		/// </summary>
		double Height { get; set; }

		/// <summary>
		///     The left coordinate of the window's position.
		/// </summary>
		double Left { get; set; }

		/// <summary>
		///     The state of the window (i.e. normal, minimized, maximized, etc...).
		/// </summary>
		WindowState State { get; set; }

		/// <summary>
		///     The top coordinate of the window's position.
		/// </summary>
		double Top { get; set; }

		/// <summary>
		///     The width of the window.
		/// </summary>
		double Width { get; set; }

		string SelectedSidePanel { get; set; }


		void Save(XmlWriter writer);
		void Restore(XmlReader reader);
		void UpdateFrom(Window window);
		void RestoreTo(Window window);
	}
}