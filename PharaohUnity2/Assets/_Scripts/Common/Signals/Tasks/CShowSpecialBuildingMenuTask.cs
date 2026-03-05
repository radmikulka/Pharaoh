// =========================================
// AUTHOR: Marek Karaba
// DATE:   15.09.2025
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class CShowSpecialBuildingMenuTask
	{
		public readonly int Index;
		public readonly int TabIndex;

		public CShowSpecialBuildingMenuTask(int index)
		{
			Index = index;
			TabIndex = -1;
		}
		
		public CShowSpecialBuildingMenuTask(int index, int tabIndex)
		{
			Index = index;
			TabIndex = tabIndex;
		}
	}
}