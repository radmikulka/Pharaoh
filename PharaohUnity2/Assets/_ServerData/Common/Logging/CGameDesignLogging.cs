// =========================================
// AUTHOR: Marek Karaba
// DATE:   19.09.2025
// =========================================

namespace ServerData.Logging
{
	public class CGameDesignLogging : IGameDesignLogging
	{
		private readonly IVehicleDataLogging _vehicleDataLogging;
		private readonly IFactoriesDataLogging _factoriesDataLogging;
		private readonly ICurrencyDataLogging _currencyDataLogging;
		
		// Pokud chces vypis logu pro kontrolu gamedesignu tak dej true. 
		// V produkci musí být vždy false!!
		private readonly bool _enabled = false;

		public CGameDesignLogging(IVehicleDataLogging vehicleDataLogging, IFactoriesDataLogging factoriesDataLogging, ICurrencyDataLogging currencyDataLogging)
		{
			_vehicleDataLogging = vehicleDataLogging;
			_factoriesDataLogging = factoriesDataLogging;
			_currencyDataLogging = currencyDataLogging;
		}


		public bool GetEnabled() => _enabled;
		
		public void Check()
		{
			//LogAllVehiclesUpgradeCost(ERegion.Region1);
			//LogAllFactoryLevels();
			//LogSoftCurrencySourcesAndSinks();
			//LogFullUpgradeCost(EVehicle.Car1);
			//VehicleExportToCsv();
		}

		private void LogAllVehiclesUpgradeCost(ERegion regionId)
		{
			_vehicleDataLogging.LogAllVehiclesUpgradeCost(regionId);
		}

		private void LogAllFactoryLevels()
		{
			_factoriesDataLogging.LogAllFactoryLevels();
		}

		private void LogFullUpgradeCost(EVehicle vehicle)
		{
			_vehicleDataLogging.LogFullUpgradeCost(vehicle);
		}

		private void LogSoftCurrencySourcesAndSinks()
		{
			_currencyDataLogging.LogCurrencySourcesAndSinks(EValuable.SoftCurrency);
		}

		private void VehicleExportToCsv()
		{
			_vehicleDataLogging.ExportAllToCsv();
		}
	}
}

