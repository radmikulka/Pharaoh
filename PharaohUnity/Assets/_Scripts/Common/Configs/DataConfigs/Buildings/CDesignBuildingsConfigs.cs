using ServerData;

namespace Pharaoh
{
    public class CDesignBuildingsConfigs : CBuildingConfigs
    {
        public CDesignBuildingsConfigs()
        {
            AddBuilding(EBuildingId.Woodcutter, () => new CBuildingConfig(
                EBuildingType.Production, ECellTag.None, "Woodcutter",
                new[]
                {
                    new SBuildingLevelData
                    {
                        Production = new[] { new SResource(EResource.Wood, 3) },
                        LevelCost = new[] { new SResource(EResource.Wood, 10) }
                    },
                    new SBuildingLevelData
                    {
                        Production = new[] { new SResource(EResource.Wood, 6) },
                        LevelCost = new[] { new SResource(EResource.Wood, 20) },
                        LevelUpRequirement = new CResearchUnlockRequirement(EResearchId.WoodcutterUpgrade)
                    }
                },
                costScalingFactor: 1.5f,
                placementRequirement: new CResearchUnlockRequirement(EResearchId.WoodcutterUnlock)
            ));

            AddBuilding(EBuildingId.StoneMason, () => new CBuildingConfig(
                EBuildingType.Production, ECellTag.None, "Stone Mason",
                new[]
                {
                    new SBuildingLevelData
                    {
                        Production = new[] { new SResource(EResource.Stone, 2) },
                        Upkeep = new[] { new SResource(EResource.Wood, 1) },
                        LevelCost = new[] { new SResource(EResource.Wood, 15) }
                    },
                    new SBuildingLevelData
                    {
                        Production = new[] { new SResource(EResource.Stone, 4) },
                        Upkeep = new[] { new SResource(EResource.Wood, 2) },
                        LevelCost = new[] { new SResource(EResource.Wood, 30) }
                    }
                },
                costScalingFactor: 1.5f
            ));

            AddBuilding(EBuildingId.Farm, () => new CBuildingConfig(
                EBuildingType.Production, ECellTag.None, "Farm",
                new[]
                {
                    new SBuildingLevelData
                    {
                        Production = new[] { new SResource(EResource.Stone, 1) },
                        LevelCost = new[] { new SResource(EResource.Wood, 12) }
                    },
                    new SBuildingLevelData
                    {
                        Production = new[] { new SResource(EResource.Stone, 2) },
                        LevelCost = new[] { new SResource(EResource.Wood, 24) }
                    }
                },
                costScalingFactor: 1.5f,
                placementRequirement: new CResearchUnlockRequirement(EResearchId.FarmUnlock)
            ));

            AddBuilding(EBuildingId.House, () => new CBuildingConfig(
                EBuildingType.Residential, ECellTag.None, "House",
                new[]
                {
                    new SBuildingLevelData
                    {
                        LevelCost = new[]
                        {
                            new SResource(EResource.Wood, 20),
                            new SResource(EResource.Stone, 5)
                        }
                    },
                    new SBuildingLevelData
                    {
                        LevelCost = new[]
                        {
                            new SResource(EResource.Wood, 40),
                            new SResource(EResource.Stone, 10)
                        }
                    }
                },
                costScalingFactor: 1.5f,
                placementRequirement: new CResearchUnlockRequirement(EResearchId.HouseUnlock)
            ));

            AddBuilding(EBuildingId.Quarry, () => new CBuildingConfig(
                EBuildingType.Mining, ECellTag.NearRock, "Quarry",
                new[]
                {
                    new SBuildingLevelData
                    {
                        Production = new[] { new SResource(EResource.Stone, 4) },
                        LevelCost = new[]
                        {
                            new SResource(EResource.Wood, 15),
                            new SResource(EResource.Stone, 5)
                        }
                    },
                    new SBuildingLevelData
                    {
                        Production = new[] { new SResource(EResource.Stone, 8) },
                        LevelCost = new[]
                        {
                            new SResource(EResource.Wood, 30),
                            new SResource(EResource.Stone, 10)
                        }
                    }
                },
                costScalingFactor: 1.5f
            ));

            AddBuilding(EBuildingId.Fishery, () => new CBuildingConfig(
                EBuildingType.Production, ECellTag.NearWater, "Fishery",
                new[]
                {
                    new SBuildingLevelData
                    {
                        Production = new[] { new SResource(EResource.Stone, 2) },
                        LevelCost = new[] { new SResource(EResource.Wood, 15) }
                    },
                    new SBuildingLevelData
                    {
                        Production = new[] { new SResource(EResource.Stone, 4) },
                        LevelCost = new[] { new SResource(EResource.Wood, 30) }
                    }
                },
                costScalingFactor: 1.5f
            ));
        }
    }
}
