// =========================================
// DATE:   02.03.2026
// =========================================

namespace ServerData
{
	public class CUnlockBuildingResearchEffect : IResearchEffect
	{
		public readonly EBuildingId BuildingId;

		public CUnlockBuildingResearchEffect(EBuildingId buildingId)
		{
			BuildingId = buildingId;
		}
	}
}
