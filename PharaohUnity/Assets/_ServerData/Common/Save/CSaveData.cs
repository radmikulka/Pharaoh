// =========================================
// DATE:   01.03.2026
// =========================================

using System.Collections.Generic;

namespace ServerData
{
    public class CSaveData
    {
        /// <summary>
        /// Revealed Voronoi cloud regions per mission.
        /// Key = (int)EMissionId, Value = set of revealed VoronoiRegionIds.
        /// </summary>
        public Dictionary<int, HashSet<int>> RevealedCloudRegions = new();

        /// <summary>
        /// Research save data per mission.
        /// Key = (int)EMissionId.
        /// </summary>
        public Dictionary<int, CMissionResearchSaveData> MissionResearch = new();
    }
}
