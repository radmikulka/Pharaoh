// =========================================
// AUTHOR: Radek Mikulka
// DATE:   08.09.2025
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class CBuildingPlot
	{
		public readonly int Index;
		public bool IsUnlocked { get; private set; }
		public ESpecialBuilding Building { get; private set; }
		public bool IsEmpty => Building == ESpecialBuilding.None;

		public CBuildingPlot(int index, bool isUnlocked, ESpecialBuilding building)
		{
			Index = index;
			IsUnlocked = isUnlocked;
			Building = building;
		}
		
		public void Unlock()
		{
			IsUnlocked = true;
		}
		
		public void PlaceSpecialBuilding(ESpecialBuilding specialBuilding)
		{
			Building = specialBuilding;
		}
	}
}