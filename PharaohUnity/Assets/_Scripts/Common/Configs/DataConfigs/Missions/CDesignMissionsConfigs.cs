using ServerData;

namespace Pharaoh
{
    public class CDesignMissionsConfigs : CMissionConfigs
    {
        public CDesignMissionsConfigs()
        {
            AddMission(EMissionId.Mission1_1, () => new CMissionData(
                ESceneId.Mission1_1,
                new[] { EBuildingId.Woodcutter, EBuildingId.Farm, EBuildingId.House },
                new[] { EResource.Wood, EResource.Stone, EResource.KnowledgePoints },
                new[]
                {
                    new CQuestChapterConfig
                    {
                        ChapterName = "Foundations",
                        Quests = new CQuestConfigBase[]
                        {
                            new CHarvestResourceQuestConfig
                            {
                                Description = "Harvest 50 Wood",
                                Resource = EResource.Wood,
                                Amount = 50,
                                Reward = new[] { new SResource(EResource.Gold, 10) }
                            },
                            new CHaveBuildingsQuestConfig
                            {
                                Description = "Place 2 Farms",
                                BuildingId = EBuildingId.Farm,
                                Count = 2,
                                Reward = new[] { new SResource(EResource.Wood, 30) }
                            },
                            new CHarvestResourceQuestConfig
                            {
                                Description = "Harvest 30 Stone",
                                Resource = EResource.Stone,
                                Amount = 30,
                                Reward = new[] { new SResource(EResource.Gold, 10) }
                            },
                            new CUpgradeBuildingQuestConfig
                            {
                                Description = "Upgrade any building",
                                BuildingId = default,
                                TargetLevel = 0,
                                Count = 1,
                                Reward = new[] { new SResource(EResource.Gold, 15) }
                            },
                            new CSpendResourceQuestConfig
                            {
                                Description = "Spend 100 Wood",
                                Resource = EResource.Wood,
                                Amount = 100,
                                Reward = new[] { new SResource(EResource.Stone, 20) }
                            },
                            new CHaveBuildingsQuestConfig
                            {
                                Description = "Build 2 Houses",
                                BuildingId = EBuildingId.House,
                                Count = 2,
                                Reward = new[] { new SResource(EResource.Gold, 20) }
                            },
                        },
                        MilestoneRewards = new[]
                        {
                            new[] { new SResource(EResource.Gold, 25) },
                            new[] { new SResource(EResource.Gold, 25), new SResource(EResource.Wood, 50) },
                            new[] { new SResource(EResource.Gold, 50) },
                        }
                    },
                    new CQuestChapterConfig
                    {
                        ChapterName = "Growth",
                        Quests = new CQuestConfigBase[]
                        {
                            new CHarvestResourceQuestConfig
                            {
                                Description = "Harvest 100 Wood",
                                Resource = EResource.Wood,
                                Amount = 100,
                                Reward = new[] { new SResource(EResource.Gold, 20) }
                            },
                            new CHaveBuildingsQuestConfig
                            {
                                Description = "Build 3 Woodcutters",
                                BuildingId = EBuildingId.Woodcutter,
                                Count = 3,
                                Reward = new[] { new SResource(EResource.Stone, 30) }
                            },
                            new CHarvestResourceQuestConfig
                            {
                                Description = "Harvest 100 Stone",
                                Resource = EResource.Stone,
                                Amount = 100,
                                Reward = new[] { new SResource(EResource.Gold, 20) }
                            },
                        },
                        MilestoneRewards = new[]
                        {
                            new[] { new SResource(EResource.Gold, 100) },
                        }
                    },
                },
                availableResearches: new CResearchConfig[]
                {
                    new(EResearchId.WoodcutterUnlock, "Woodcutter",
                        prerequisites: System.Array.Empty<EResearchId>(),
                        cost: new[] { new SResource(EResource.KnowledgePoints, 2) },
                        effects: new IResearchEffect[] { new CUnlockBuildingResearchEffect(EBuildingId.Woodcutter) }),

                    new(EResearchId.FarmUnlock, "Farm",
                        prerequisites: new[] { EResearchId.WoodcutterUnlock },
                        cost: new[] { new SResource(EResource.KnowledgePoints, 3) },
                        effects: new IResearchEffect[] { new CUnlockBuildingResearchEffect(EBuildingId.Farm) }),

                    new(EResearchId.HouseUnlock, "House",
                        prerequisites: new[] { EResearchId.WoodcutterUnlock },
                        cost: new[] { new SResource(EResource.KnowledgePoints, 3) },
                        effects: new IResearchEffect[] { new CUnlockBuildingResearchEffect(EBuildingId.House) }),

                    new(EResearchId.WoodcutterUpgrade, "Woodcutter Upgrade",
                        prerequisites: new[] { EResearchId.WoodcutterUnlock },
                        cost: new[]
                        {
                            new SResource(EResource.KnowledgePoints, 4),
                            new SResource(EResource.Wood, 50),
                        },
                        effects: new IResearchEffect[] { new CBuildingLevelResearchEffect(EBuildingId.Woodcutter, 2) }),

                    new(EResearchId.ProductionBoostI, "Production Boost I",
                        prerequisites: new[] { EResearchId.WoodcutterUpgrade },
                        cost: new[]
                        {
                            new SResource(EResource.KnowledgePoints, 5),
                            new SResource(EResource.Wood, 100),
                        },
                        effects: new IResearchEffect[] { new CProductionBonusResearchEffect(EBuildingId.Woodcutter, productionMultiplier: 1.25f) }),
                }
            ));
        }
    }
}
