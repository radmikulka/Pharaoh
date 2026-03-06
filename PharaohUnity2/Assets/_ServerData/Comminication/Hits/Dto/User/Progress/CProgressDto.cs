// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using AldaEngine;
using Newtonsoft.Json;

namespace ServerData
{
    public class CProgressDto : IMapAble
    {
        [JsonProperty] public EMissionId MissionId { get; set; }

        public CProgressDto()
        {
        }

        public CProgressDto(EMissionId missionId)
        {
            MissionId = missionId;
        }
    }
}