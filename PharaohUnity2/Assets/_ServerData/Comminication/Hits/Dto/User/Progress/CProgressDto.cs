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
        [JsonProperty] public EYearMilestone SeenYear { get; set; }
        [JsonProperty] public EYearMilestone Year { get; set; }
        [JsonProperty] public int XpInCurrentYear { get; set; }
        [JsonProperty] public ERegion Region { get; set; }

        public CProgressDto()
        {
        }

        public CProgressDto(
            EYearMilestone year, 
            ERegion region, 
            EYearMilestone seenYear, 
            int xpInCurrentYear
            )
        {
            XpInCurrentYear = xpInCurrentYear;
            SeenYear = seenYear;
            Year = year;
            Region = region;
        }
    }
}