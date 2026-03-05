// =========================================
// AUTHOR: Marek Karaba
// DATE:   14.01.2026
// =========================================

using ServerData;

namespace TycoonBuilder.Ui
{
	public class CTopBarItem : ITopBarItem
	{
		public ETopBarItem Id { get; }

		public bool ShowButton { get; }

		public CTopBarItem(ETopBarItem id, bool showButton)
		{
			Id = id;
			ShowButton = showButton;
		}
	}
}