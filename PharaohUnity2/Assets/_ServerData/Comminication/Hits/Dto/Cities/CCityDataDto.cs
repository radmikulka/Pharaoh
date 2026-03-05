// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.09.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData
{
	public class CCityDataDto
	{
		[JsonProperty] public CLevelDataDto LevelData { get; set; }
		[JsonProperty] public CRechargerDto PassengersGenerator { get; set; }
		[JsonProperty] public CBuildingPlotDto[] BuildingPlots { get; set; }

		public CCityDataDto()
		{
		}

		public CCityDataDto(
			CLevelDataDto levelData, 
			CRechargerDto passengersGenerator,
			CBuildingPlotDto[] buildingPlots
			)
		{
			PassengersGenerator = passengersGenerator;
			BuildingPlots = buildingPlots;
			LevelData = levelData;
		}
	}
}