// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.09.2025
// =========================================

namespace ServerData
{
	public class CNewFactoryProductReward : IUpgradeReward
	{
		public readonly CFactoryProductConfig ProductConfig;

		public CNewFactoryProductReward(CFactoryProductConfig productConfig)
		{
			ProductConfig = productConfig;
		}
	}
}