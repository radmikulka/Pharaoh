using ServerData;

namespace Pharaoh
{
    public class CMissionData
    {
        public ESceneId SceneId { get; }
        public EBuildingId[] AvailableBuildings { get; }
        public EResource[] AvailableResources { get; }
        public CQuestChapterConfig[] QuestChapters { get; }
        public CResearchConfig[] AvailableResearches { get; }

        public CMissionData(
            ESceneId sceneId,
            EBuildingId[] availableBuildings,
            EResource[] availableResources,
            CQuestChapterConfig[] questChapters = null,
            CResearchConfig[] availableResearches = null)
        {
            SceneId = sceneId;
            AvailableBuildings = availableBuildings;
            AvailableResources = availableResources;
            QuestChapters = questChapters ?? System.Array.Empty<CQuestChapterConfig>();
            AvailableResearches = availableResearches ?? System.Array.Empty<CResearchConfig>();
        }
    }
}
