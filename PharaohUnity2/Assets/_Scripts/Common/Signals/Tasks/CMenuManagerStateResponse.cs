// =========================================
// AUTHOR: Marek Karaba
// DATE:   24.07.2025
// =========================================

namespace TycoonBuilder
{
	public class CMenuManagerStateResponse
	{
		public readonly bool Active;
		public readonly int OpenedMenusCount;

		public CMenuManagerStateResponse(bool active, int openedMenusCount)
		{
			Active = active;
			OpenedMenusCount = openedMenusCount;
		}
	}
}