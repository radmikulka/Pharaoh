// =========================================
// AUTHOR: Marek Karaba
// DATE:   27.10.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CDecadePassPremiumPurchasedSignal : IEventBusSignal
	{
		public readonly EBattlePassPremiumStatus PremiumStatus;

		public CDecadePassPremiumPurchasedSignal(EBattlePassPremiumStatus premiumStatus)
		{
			PremiumStatus = premiumStatus;
		}
	}
}