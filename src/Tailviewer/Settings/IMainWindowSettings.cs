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

		/// <summary>
		///     Whether or not the window is always on top of other windows.
		/// </summary>
		bool AlwaysOnTop { get; set; }

		string SelectedSidePanel { get; set; }

		/// <summary>
		///    When set to true, then the left-side panel is collapsed to
		///    reduce space requirements.
		/// </summary>
		bool IsLeftSidePanelVisible { get; set; }

		string SelectedMainPanel { get; set; }

		void Save(XmlWriter writer);
		void Restore(XmlReader reader);
		void UpdateFrom(Window window);
		void ClipToBounds(Desktop desktop);
		void RestoreTo(Window window);
	}
}