// =========================================
// DATE:   02.03.2026
// =========================================

namespace ServerData
{
	public class CBuildingLevelResearchEffect : IResearchEffect
	{
		public readonly EBuildingId BuildingId;
		public readonly int MaxLevel;

		public CBuildingLevelResearchEffect(EBuildingId buildingId, int maxLevel)
		{
			BuildingId = buildingId;
			MaxLevel = maxLevel;
		}
	}
}
