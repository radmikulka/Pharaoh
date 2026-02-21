// =========================================
// AUTHOR: Radek Mikulka
// DATE:   28.11.2024
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData
{
	public class CUserDto
	{
		[JsonProperty] public CActiveMissionDto ActiveMission { get; set; }
		[JsonProperty] public COwnedValuablesDto OwnedValuables { get; set; }
		[JsonProperty] public CAccountDto Account { get; set; }

		public CUserDto(
	        COwnedValuablesDto ownedValuables,
	        CActiveMissionDto activeMission,
	        CAccountDto account
	        )
        {
	        OwnedValuables = ownedValuables;
	        ActiveMission = activeMission;
	        Account = account;
        }
	}
}