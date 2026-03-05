// =========================================
// AUTHOR: Marek Karaba
// DATE:   08.01.2026
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CEventPassPremiumPurchasedSignal : IEventBusSignal
	{
		public ELiveEvent LiveEventId { get; } 
		public EBattlePassPremiumStatus PremiumStatus { get; }

		public CEventPassPremiumPurchasedSignal(ELiveEvent liveEventId, EBattlePassPremiumStatus premiumStatus)
		{
			PremiumStatus = premiumStatus;
			LiveEventId = liveEventId;
		}
	}
}