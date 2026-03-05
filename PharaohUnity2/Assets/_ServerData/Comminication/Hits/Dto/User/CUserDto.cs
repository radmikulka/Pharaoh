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
		[JsonProperty] public CGlobalVariablesDto GlobalVariables { get; set; }
		[JsonProperty] public COwnedValuablesDto OwnedValuables { get; set; }
		[JsonProperty] public CBattlePassDataDto DecadePass { get; set; }
		[JsonProperty] public CFuelStationDto FuelStation { get; set; }
		[JsonProperty] public CVehicleDepoDto VehicleDepo { get; set; }
		[JsonProperty] public CUserTutorialsDto Tutorials { get; set; }
		[JsonProperty] public CDispatchersDto Dispatchers { get; set; }
		[JsonProperty] public COwnedFramesDto OwnedFrames { get; set; }
		[JsonProperty] public CSideCitiesDto SideCities { get; set; }
		[JsonProperty] public CLiveEventsDto LiveEvents { get; set; }
		[JsonProperty] public CDispatchesDto Dispatches { get; set; }
		[JsonProperty] public CWarehouseDto Warehouse { get; set; }
		[JsonProperty] public CContractsDto Contracts { get; set; }
		[JsonProperty] public CFactoriesDto Factories { get; set; }
		[JsonProperty] public CDebugInfoDto DebugInfo { get; set; }
		[JsonProperty] public CProgressDto Progress { get; set; }
		[JsonProperty] public CVehiclesDto Vehicles { get; set; }
		[JsonProperty] public CProjectsDto Projects { get; set; }
		[JsonProperty] public CAccountDto Account { get; set; }
		[JsonProperty] public COffersDto Offers { get; set; }
		[JsonProperty] public CCitiesDto Cities { get; set; }
		[JsonProperty] public CTasksDto Tasks { get; set; }

		public CUserDto(
	        CGlobalVariablesDto globalVariables,
	        COwnedValuablesDto ownedValuables,
	        CBattlePassDataDto decadePass,
	        CAccountDto account, 
	        CProgressDto progress, 
	        COffersDto offers,
	        CVehiclesDto vehicles,
	        CContractsDto contracts,
	        CWarehouseDto warehouse,
	        CDispatchesDto dispatches,
	        CCitiesDto cities,
	        CFactoriesDto factories,
	        CFuelStationDto fuelStation, 
	        CVehicleDepoDto vehicleDepo,
	        CUserTutorialsDto tutorials,
	        CDebugInfoDto debugInfo,
	        CLiveEventsDto liveEvents,
	        CSideCitiesDto sideCities,
	        CDispatchersDto dispatchers,
	        COwnedFramesDto ownedFrames,
	        CTasksDto tasks, 
	        CProjectsDto projects
	        )
        {
	        GlobalVariables = globalVariables;
	        OwnedValuables = ownedValuables;
	        Dispatchers = dispatchers;
	        FuelStation = fuelStation;
	        VehicleDepo = vehicleDepo;
	        OwnedFrames = ownedFrames;
	        DecadePass = decadePass;
	        Dispatches = dispatches;
	        LiveEvents = liveEvents;
	        SideCities = sideCities;
	        Contracts = contracts;
	        Warehouse = warehouse;
	        Factories = factories;
	        Tutorials = tutorials;
	        DebugInfo = debugInfo;
	        Vehicles = vehicles;
	        Progress = progress;
	        Projects = projects;
	        Account = account;
	        Cities = cities;
	        Offers = offers;
	        Tasks = tasks;
        }
	}
}