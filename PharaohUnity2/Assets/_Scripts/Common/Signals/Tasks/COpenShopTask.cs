// =========================================
// AUTHOR: Juraj Joscak
// DATE:   22.07.2025
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public enum EShopTab
	{
		None = 0,
		SpecialOffers = 1,
		HardAndSoftCurrency = 2,
		DailyOffers = 3,
		Maintenance = 4,
		Dispatchers = 5,
	}
	
	
	public class COpenShopTask
	{
		public readonly EShopTab Tab;
		public readonly EValuable TargetReward;
		
		public COpenShopTask(EShopTab tab, EValuable targetReward)
		{
			Tab = tab;
			TargetReward = targetReward;
		}
	}
}