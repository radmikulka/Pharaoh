using System;
using ServerData;

namespace Pharaoh
{
    public class CBuildingConfig
    {
        public EBuildingType BuildingType { get; }
        public ECellTag RequiredTags { get; }
        public string DisplayName { get; }
        public SBuildingLevelData[] Levels { get; }
        public float CostScalingFactor { get; }
        public IUnlockRequirement PlacementRequirement { get; }

        public CBuildingConfig(EBuildingType buildingType, ECellTag requiredTags, string displayName, SBuildingLevelData[] levels, float costScalingFactor = 1.5f, IUnlockRequirement placementRequirement = null)
        {
            BuildingType = buildingType;
            RequiredTags = requiredTags;
            DisplayName = displayName;
            Levels = levels;
            CostScalingFactor = costScalingFactor;
            PlacementRequirement = placementRequirement ?? IUnlockRequirement.Null();
        }

        public SBuildingLevelData GetLevelData(int level)
        {
            if (Levels == null || Levels.Length == 0)
                return default;
            int index = Math.Clamp(level - 1, 0, Levels.Length - 1);
            return Levels[index];
        }

        public SResource[] GetBuildCost(int ownedCount)
        {
            if (Levels == null || Levels.Length == 0 || Levels[0].LevelCost == null)
                return Array.Empty<SResource>();

            SResource[] baseCost = Levels[0].LevelCost;
            float multiplier = (float)Math.Pow(CostScalingFactor, ownedCount);

            SResource[] scaled = new SResource[baseCost.Length];
            for (int i = 0; i < baseCost.Length; i++)
            {
                scaled[i] = new SResource(baseCost[i].Id, Math.Max(1, (int)(baseCost[i].Amount * multiplier)));
            }
            return scaled;
        }
    }
}
