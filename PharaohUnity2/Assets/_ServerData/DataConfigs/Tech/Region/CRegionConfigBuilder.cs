// =========================================
// AUTHOR: Radek Mikulka
// DATE:   26.06.2025
// =========================================

using System;
using System.CodeDom;
using System.Collections.Generic;

namespace ServerData
{
	public class CRegionConfigBuilder
	{
		private readonly ERegion _regionId;

		private EBundleId _contentBundleId;
		private EBundleId _enviroBundleId;
		private float _fuelIncomeModificator;
		private float _passengerIncomeModificator;
		private float _softCurrencyIncomeModificator;
		private float _softCurrencySpendModificator;
		private float _vehicleUpgradePartsPriceModificator;
		private float _vehicleUpgradeSoftCurrencyPriceModificator;
		private float _vehiclePartsRewardAmountModificator;
		private float _eventPassengerContractRequirementModifier;
		private readonly List<CRetroactiveTaskConfig> _decadeChecklistTasks = new();

		private CDecadePassConfig _decadePassConfig;

		public CRegionConfigBuilder(ERegion regionId)
		{
			_regionId = regionId;
		}
		
		public CRegionConfigBuilder SetSoftCurrencyIncomeModificator(float modificator)
		{
			_softCurrencyIncomeModificator = modificator;
			return this;
		}

		public CRegionConfigBuilder SetPassengerIncomeModificator(float modificator)
		{
			_passengerIncomeModificator = modificator;
			return this;
		}

		public CRegionConfigBuilder SetFuelIncomeModificator(float modificator)
		{
			_fuelIncomeModificator = modificator;
			return this;
		}
		
		public CRegionConfigBuilder AddDecadeChecklistTask(CRetroactiveTaskConfig taskConfig)
		{
			_decadeChecklistTasks.Add(taskConfig);
			return this;
		}
		
		public CRegionConfigBuilder SetEventPassengerContractRequirementModifier(float modificator)
		{
			_eventPassengerContractRequirementModifier = modificator;
			return this;
		}
		
		public CRegionConfigBuilder SetSoftCurrencySpendModificator(float modificator)
		{
			_softCurrencySpendModificator = modificator;
			return this;
		}

		public CRegionConfigBuilder SetContentBundleId(EBundleId bundleId)
		{
			_contentBundleId = bundleId;
			return this;
		}
		
		public CRegionConfigBuilder SetEnvironmentBundleId(EBundleId bundleId)
		{
			_enviroBundleId = bundleId;
			return this;
		}

		public CRegionConfigBuilder SetVehicleUpgradePartsPriceModificator(float modificator)
		{
			_vehicleUpgradePartsPriceModificator = modificator;
			return this;
		}
		
		public CRegionConfigBuilder SetVehicleUpgradeSoftCurrencyPriceModificator(float modificator)
		{
			_vehicleUpgradeSoftCurrencyPriceModificator = modificator;
			return this;
		}
		
		public CRegionConfigBuilder SetVehiclePartsRewardAmountModificator(float modificator)
		{
			_vehiclePartsRewardAmountModificator = modificator;
			return this;
		}
		
		public CRegionConfigBuilder SetDecadePassConfig(Func<CDecadePassConfigData> configBuilder)
		{
			_decadePassConfig = new CDecadePassConfig(configBuilder);
			return this;
		}
		
		public CRegionConfig Build()
		{
			return new CRegionConfig(
				_regionId, 
				_decadeChecklistTasks.ToArray(),
				_decadePassConfig,
				_softCurrencyIncomeModificator,
				_softCurrencySpendModificator,
				_vehicleUpgradePartsPriceModificator,
				_vehicleUpgradeSoftCurrencyPriceModificator,
				_vehiclePartsRewardAmountModificator,
				_eventPassengerContractRequirementModifier,
				_fuelIncomeModificator,
				_passengerIncomeModificator,
				_contentBundleId,
				_enviroBundleId
				);
		}
	}
}