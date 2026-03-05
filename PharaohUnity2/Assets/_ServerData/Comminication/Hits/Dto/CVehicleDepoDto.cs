// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.09.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData
{
	public class CVehicleDepoDto
	{
		[JsonProperty] public CLevelDataDto LevelData { get; set; }

		public CVehicleDepoDto()
		{
		}

		public CVehicleDepoDto(CLevelDataDto levelData)
		{
			LevelData = levelData;
		}
	}
}