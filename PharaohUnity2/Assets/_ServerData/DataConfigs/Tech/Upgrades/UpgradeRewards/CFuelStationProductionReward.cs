// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.09.2025
// =========================================

namespace ServerData
{
	public class CFuelStationProductionReward : IUpgradeReward
	{
		public readonly int ProductionIncrease;

		public CFuelStationProductionReward(int productionIncrease)
		{
			ProductionIncrease = productionIncrease;
		}
	}
}