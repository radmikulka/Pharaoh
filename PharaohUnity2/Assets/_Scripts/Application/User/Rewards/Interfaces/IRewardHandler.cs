// =========================================
// AUTHOR: Marek Karaba
// DATE:   18.07.2025
// =========================================

using System.Threading;
using Cysharp.Threading.Tasks;
using ServerData;
using TycoonBuilder.Infrastructure;

namespace TycoonBuilder
{
	public interface IRewardHandler
	{
		UniTask ClaimRewards(
			IValuable[] rewards, 
			IParticleSource[] sources, 
			EModificationSource source, 
			bool shouldBlockQueue, 
			CancellationToken ct, 
			CValueModifyParams modifyParams
			);
		
		UniTask ClaimOffer(
			COfferWithResourceSources offerWithSources, 
			EModificationSource priceSource, 
			EModificationSource rewardSource, 
			bool shouldBlockQueue, 
			CPurchasePayloads payloads,
			CancellationToken ct
			);
		
		void ChargeValuables(IValuable[] rewards, EModificationSource source, CValueModifyParams modifyParams);
	}
}