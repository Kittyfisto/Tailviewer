using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse
{
	/// <summary>
	///     Represents a running analysis of one or more data sources.
	/// </summary>
	public interface IAnalysisViewModel
		: INotifyPropertyChanged
	{
		AnalysisId Id { get; }
		IEnumerable<IAnalysisPageViewModel> Pages { get; }
		bool IsSelected { get; set; }
		string Name { get; set; }
		ICommand AddPageCommand { get; }
		double Progress { get; }

		void Add(ILogFile logFile);
		void Remove(ILogFile logFile);
	}
}