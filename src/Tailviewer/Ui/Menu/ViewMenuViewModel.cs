using Tailviewer.Ui.DataSourceTree;

namespace Tailviewer.Ui.Menu
{
	public sealed class ViewMenuViewModel
		: AbstractMainMenuViewModel
	{
		#region Implementation of IMenu

		public override IDataSourceViewModel CurrentDataSource
		{
			set
			{
				if (AllItems.ChildCollectionCount > 0)
				{
					AllItems.RemoveAt(0);
				}

				AllItems.Insert(0, value?.ViewMenuItems);
			}
		}

		#endregion
	}
}
