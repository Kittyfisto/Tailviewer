using System.Windows;
using System.Windows.Controls;

namespace Tailviewer.Ui
{
	/// <summary>
	///     This control can be used to explain to the user that a collection (a list view, etc...) is empty as well as why it
	///     is empty and what can be done to remedy that.
	/// </summary>
	public sealed class EmptyState
		: Control
	{
		public static readonly DependencyProperty EmptyStatementProperty =
			DependencyProperty.Register("EmptyStatement", typeof(string), typeof(EmptyState),
			                            new PropertyMetadata(default(string)));

		public static readonly DependencyProperty EmptyExplanationProperty = DependencyProperty.Register(
		 "EmptyExplanation", typeof(string), typeof(EmptyState), new PropertyMetadata(default(string)));

		static EmptyState()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(EmptyState),
			                                         new FrameworkPropertyMetadata(typeof(EmptyState)));
		}

		/// <summary>
		///     Explains that the collection is empty. MUST be short (single sentence only, ONE line).
		/// </summary>
		/// <remarks>
		///     Should read something like "There are no X", "No results for search Y", etc...
		/// </remarks>
		public string EmptyStatement

		{
			get { return (string) GetValue(EmptyStatementProperty); }
			set { SetValue(EmptyStatementProperty, value); }
		}

		/// <summary>
		///     A more detailed explanation of why the collection is empty.
		/// </summary>
		public string EmptyExplanation
		{
			get { return (string) GetValue(EmptyExplanationProperty); }
			set { SetValue(EmptyExplanationProperty, value); }
		}
	}
}