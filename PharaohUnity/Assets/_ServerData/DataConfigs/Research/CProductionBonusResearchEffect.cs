// =========================================
// DATE:   02.03.2026
// =========================================

namespace ServerData
{
	public class CProductionBonusResearchEffect : IResearchEffect
	{
		public readonly EBuildingId BuildingId;
		public readonly float ProductionMultiplier;
		public readonly float SpeedMultiplier;

		public CProductionBonusResearchEffect(EBuildingId buildingId, float productionMultiplier = 1f, float speedMultiplier = 1f)
		{
			BuildingId = buildingId;
			ProductionMultiplier = productionMultiplier;
			SpeedMultiplier = speedMultiplier;
		}
	}
}
