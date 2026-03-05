// =========================================
// AUTHOR: Marek Karaba
// DATE:   01.08.2025
// =========================================

using System.Threading;
using Cysharp.Threading.Tasks;
using ServerData;

namespace TycoonBuilder
{
	public interface IRewardQueue
	{
		void AddRewards(EModificationSource source, IValuable[] rewards, CValueModifyParams modifyParams = null);
		
		void AddRewardsWithSources(
			EModificationSource source, 
			IValuable[] rewards, 
			IParticleSource[] particleSources,
			CValueModifyParams modifyParams = null
			);
		
		void AddOfferRewardsWithSources(
			EModificationSource priceSource, 
			EModificationSource rewardSource, 
			string offerGuid, 
			CPurchasePayloads payloads,
			CValueModifyParams modifyParams = null
			);
		
		void AddOfferRewardsWithSources(
			EModificationSource priceSource, 
			EModificationSource rewardSource, 
			string offerGuid, 
			IParticleSource[] particleSources, 
			CPurchasePayloads payloads,
			CValueModifyParams modifyParams = null
			);
		
		void ChargeValuable(EModificationSource source, IValuable[] rewards, CValueModifyParams modifyParams = null);
		
		UniTask WaitUntilQueueUnblocked(CancellationToken ct);
		
		UniTask WaitWhileIsRunning(CancellationToken ct) => UniTask.WaitWhile(IsRunning, cancellationToken: ct);
		
		bool IsRunning();
	}
}
