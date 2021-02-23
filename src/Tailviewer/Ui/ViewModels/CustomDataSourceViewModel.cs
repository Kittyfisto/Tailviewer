using System.Windows.Input;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.Settings;

namespace Tailviewer.Ui.ViewModels
{
	public sealed class CustomDataSourceViewModel
		: AbstractDataSourceViewModel
		, ISingleDataSourceViewModel
	{
		public CustomDataSourceViewModel(IDataSource dataSource,
		                                 IActionCenter actionCenter,
		                                 IApplicationSettings applicationSettings) : base(dataSource, actionCenter, applicationSettings)
		{
		}

		#region Overrides of AbstractDataSourceViewModel

		public override string DisplayName
		{
			get { throw new System.NotImplementedException(); }
			set { throw new System.NotImplementedException(); }
		}

		public override bool CanBeRenamed
		{
			get { throw new System.NotImplementedException(); }
		}

		public override string DataSourceOrigin
		{
			get { throw new System.NotImplementedException(); }
		}

		#endregion

		#region Implementation of ISingleDataSourceViewModel

		public bool CanBeRemoved
		{
			get { throw new System.NotImplementedException(); }
		}

		public string CharacterCode
		{
			get { throw new System.NotImplementedException(); }
			set { throw new System.NotImplementedException(); }
		}

		public bool ExcludeFromParent
		{
			get { throw new System.NotImplementedException(); }
			set { throw new System.NotImplementedException(); }
		}

		#endregion
	}
}