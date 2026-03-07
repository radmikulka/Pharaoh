// =========================================
// AUTHOR: Marek Karaba
// DATE:   24.07.2025
// =========================================

namespace Pharaoh
{
	public class CMenuStateResponse
	{
		public readonly bool Active;
		public readonly int OpenedMenusCount;

		public CMenuStateResponse(bool active, int openedMenusCount)
		{
			Active = active;
			OpenedMenusCount = openedMenusCount;
		}
	}
}