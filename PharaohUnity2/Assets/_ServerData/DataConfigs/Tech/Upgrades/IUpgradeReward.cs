// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.09.2025
// =========================================

namespace ServerData
{
	public interface IUpgradeReward
	{
		public static CNewFactoryProductReward FactoryProduct(CFactoryProductConfig config)
		{
			return new CNewFactoryProductReward(config);
		}
		
		public static CFactoryMaxDurabilityReward FactoryDurability(int durabilityIncrease)
		{
			return new CFactoryMaxDurabilityReward(durabilityIncrease);
		}
		
		public static CFactoryRepairSpeedReward FactoryRepairSpeed(int repairAmountIncrease, int repairTimeDecreaseInSeconds)
		{
			return new CFactoryRepairSpeedReward(repairAmountIncrease, repairTimeDecreaseInSeconds);
		}
		
		public static CFuelStationCapacityReward FuelStationCapacity(int capacityIncreasePercent)
		{
			return new CFuelStationCapacityReward(capacityIncreasePercent);
		}
		
		public static CFuelStationProductionReward FuelStationProduction(int productionIncrease)
		{
			return new CFuelStationProductionReward(productionIncrease);
		}
		
		public static CVehicleDurabilityRepairAmountReward VehicleDurabilityRepairAmountIncrease(int amountIncrease)
		{
			return new CVehicleDurabilityRepairAmountReward(amountIncrease);
		}
	}
}