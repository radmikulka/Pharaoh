// =========================================
// AUTHOR: Radek Mikulka
// DATE:   10.09.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData
{
	public class CFuelStationDto
	{
		[JsonProperty] public CLevelDataDto LevelData { get; set; }
		[JsonProperty] public CRechargerDto Recharger { get; set; }

		public CFuelStationDto()
		{
		}

		public CFuelStationDto(CLevelDataDto levelData, CRechargerDto recharger)
		{
			LevelData = levelData;
			Recharger = recharger;
		}
	}
}