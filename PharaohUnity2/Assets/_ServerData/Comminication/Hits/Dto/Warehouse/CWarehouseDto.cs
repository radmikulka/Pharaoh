// =========================================
// AUTHOR: Radek Mikulka
// DATE:   10.07.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData
{
	public class CWarehouseDto
	{
		[JsonProperty] public CLevelDataDto LevelData { get; set; }
		[JsonProperty] public SResource[] Resources { get; set; }

		public CWarehouseDto()
		{
		}

		public CWarehouseDto(CLevelDataDto levelData, SResource[] resources)
		{
			LevelData = levelData;
			Resources = resources;
		}
	}
}