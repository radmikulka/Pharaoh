// =========================================
// AUTHOR: Radek Mikulka
// DATE:   08.09.2025
// =========================================

using AldaEngine;
using Newtonsoft.Json;

namespace ServerData
{
	public class CBuildingPlotDto : IMapAble
	{
		[JsonProperty] public int Index { get; set; }
		[JsonProperty] public bool IsUnlocked { get; set; }
		[JsonProperty] public ESpecialBuilding Building { get; set; }

		public CBuildingPlotDto()
		{
		}

		public CBuildingPlotDto(int index, bool isUnlocked, ESpecialBuilding building)
		{
			Index = index;
			IsUnlocked = isUnlocked;
			Building = building;
		}
	}
}