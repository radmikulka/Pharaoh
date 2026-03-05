// =========================================
// AUTHOR: Marek Karaba
// DATE:   14.01.2026
// =========================================

using ServerData;

namespace TycoonBuilder.Ui
{
	public interface ITopBarItem
	{
		public ETopBarItem Id { get; }
		public bool ShowButton { get; }
	}
}