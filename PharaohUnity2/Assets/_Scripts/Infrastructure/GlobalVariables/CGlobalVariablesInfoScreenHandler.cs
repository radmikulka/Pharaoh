// =========================================
// AUTHOR: Marek Karaba
// DATE:   20.11.2025
// =========================================

using AldaEngine;
using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	public class CGlobalVariablesInfoScreenHandler
	{
		private readonly CGlobalVariablesHandler _globalVariablesHandler;
		private readonly IEventBus _eventBus;

		public CGlobalVariablesInfoScreenHandler(CGlobalVariablesHandler globalVariablesHandler, IEventBus eventBus)
		{
			_globalVariablesHandler = globalVariablesHandler;
			_eventBus = eventBus;
		}

		public void SetGlobalVariable(EScreenInfoId screenInfoId)
		{
			EGlobalVariable globalVariable = GetGlobalVariableForContent(screenInfoId);

			if (globalVariable == EGlobalVariable.None)
			{
				Debug.LogError($"Global variable for EScreenInfoId {screenInfoId} not set.");
				return;
			}
			
			_globalVariablesHandler.SaveGlobalVariableBool(globalVariable, true);
			_eventBus.Send(new CScreenInfoSeenSignal(screenInfoId));
		}

		public bool GetGlobalVariable(EScreenInfoId screenInfoId)
		{
			EGlobalVariable globalVariable = GetGlobalVariableForContent(screenInfoId);
			
			if (globalVariable == EGlobalVariable.None)
			{
				Debug.LogError($"Global variable for EScreenInfoId {screenInfoId} not set.");
				return false;
			}
			
			return _globalVariablesHandler.GetGlobalVariableBool(globalVariable);
		}

		private EGlobalVariable GetGlobalVariableForContent(EScreenInfoId screenInfoId)
		{
			switch (screenInfoId)
			{
				case EScreenInfoId.DecadePass:
					return EGlobalVariable.DecadePassInfoScreenSeen;
				case EScreenInfoId.CityMenu:
					return EGlobalVariable.CityMenuInfoScreenSeen;
				case EScreenInfoId.Parcels:
					return EGlobalVariable.ParcelsInfoScreenSeen;
				case EScreenInfoId.UpgradeVehicles:
					return EGlobalVariable.UpgradeVehiclesInfoScreenSeen;
				case EScreenInfoId.EventOverview:
					return EGlobalVariable.EventOverviewInfoScreenSeen;
				case EScreenInfoId.EventPass:
					return EGlobalVariable.EventPassInfoScreenSeen;
				case EScreenInfoId.Factories:
					return EGlobalVariable.FactoriesInfoScreenSeen;
				case EScreenInfoId.WarehouseUpgrade:
					return EGlobalVariable.WarehouseUpgradeInfoScreenSeen;
				case EScreenInfoId.CityUpgrade:
					return EGlobalVariable.CityUpgradeInfoScreenSeen;
				case EScreenInfoId.DepotUpgrade:
					return EGlobalVariable.DepotUpgradeInfoScreenSeen;
				case EScreenInfoId.FuelStationUpgrade:
					return EGlobalVariable.FuelStationUpgradeInfoScreenSeen;
				case EScreenInfoId.FactoryUpgrade:
					return EGlobalVariable.FactoryUpgradeInfoScreenSeen;
				default:
					return EGlobalVariable.None;
			}
		}
	}
}