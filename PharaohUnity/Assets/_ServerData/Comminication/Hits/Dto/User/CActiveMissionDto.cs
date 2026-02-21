using Newtonsoft.Json;

namespace ServerData
{
    public class CActiveMissionDto
    {
		[JsonProperty] public EMissionId MissionId { get; set; }

        public CActiveMissionDto()
        {
        }

        public CActiveMissionDto(EMissionId missionId)
        {
            MissionId = missionId;
        }
    }
}