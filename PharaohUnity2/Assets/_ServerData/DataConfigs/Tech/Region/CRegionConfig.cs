// =========================================
// AUTHOR: Radek Mikulka
// DATE:   26.06.2025
// =========================================

using System;
using System.Collections.Generic;

namespace ServerData
{
	public class CRegionConfig
	{
		private readonly CRetroactiveTaskConfig[] _decadeChecklistTasks;
		
		public readonly ERegion Id;
		public readonly EBundleId ContentBundleId;
		public readonly EBundleId EnviroBundle;
		public readonly float SoftCurrencyIncomeModificator;
		public readonly float SoftCurrencySpendModificator;
		public readonly float VehicleUpgradePartsPriceModificator;
		public readonly float VehicleUpgradeSoftCurrencyPriceModificator;
		public readonly float VehiclePartsRewardAmountModificator;
		public readonly float FuelIncomeModificator;
		public readonly float PassengerIncomeModificator;
		public readonly float EventPassengerContractRequirementModifier;
		private readonly CDecadePassConfig _decadePassConfig;
		
		public IReadOnlyList<CRetroactiveTaskConfig> DecadeChecklistTasks => _decadeChecklistTasks;

		public CRegionConfig(
			ERegion id, 
			CRetroactiveTaskConfig[] decadeChecklistTasks,
			CDecadePassConfig decadePassConfig,
			float softCurrencyIncomeModificator, 
			float softCurrencySpendModificator, 
			float vehicleUpgradePartsPriceModificator, 
			float vehicleUpgradeSoftCurrencyPriceModificator, 
			float vehiclePartsRewardAmountModificator,
			float eventPassengerContractRequirementModifier,
			float fuelIncomeModificator,
			float passengerIncomeModificator,
			EBundleId contentBundleId,
			EBundleId enviroBundle
			)
		{
			VehicleUpgradeSoftCurrencyPriceModificator = vehicleUpgradeSoftCurrencyPriceModificator;
			EventPassengerContractRequirementModifier = eventPassengerContractRequirementModifier;
			VehicleUpgradePartsPriceModificator = vehicleUpgradePartsPriceModificator;
			VehiclePartsRewardAmountModificator = vehiclePartsRewardAmountModificator;
			SoftCurrencyIncomeModificator = softCurrencyIncomeModificator;
			SoftCurrencySpendModificator = softCurrencySpendModificator;
			PassengerIncomeModificator = passengerIncomeModificator;
			FuelIncomeModificator = fuelIncomeModificator;
			_decadeChecklistTasks = decadeChecklistTasks;
			_decadePassConfig = decadePassConfig;
			ContentBundleId = contentBundleId;
			EnviroBundle = enviroBundle;
			Id = id;
		}

		public CDecadePassConfigData GetDecadePass()
		{
			return _decadePassConfig.GetConfig();
		}
	}
}