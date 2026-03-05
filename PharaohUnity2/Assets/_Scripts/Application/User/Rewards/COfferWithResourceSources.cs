// =========================================
// AUTHOR: Marek Karaba
// DATE:   01.08.2025
// =========================================

using ServerData;

namespace TycoonBuilder.Infrastructure
{
	public class COfferWithResourceSources
	{
		public readonly IParticleSource[] ParticleSources;
		public readonly EModificationSource RewardSource;
		public readonly EModificationSource PriceSource;
		public readonly string OfferGuid;
		public readonly CValueModifyParams ModifyParams;
		public readonly CPurchasePayloads Payloads;
		

		public COfferWithResourceSources(
			string offerGuid, 
			IParticleSource[] particleSources, 
			EModificationSource priceSource, 
			EModificationSource rewardSource, 
			CValueModifyParams modifyParams, 
			CPurchasePayloads payloads
			)
		{
			ParticleSources = particleSources;
			RewardSource = rewardSource;
			ModifyParams = modifyParams;
			Payloads = payloads ?? CPurchasePayloads.Empty;
			PriceSource	= priceSource;
			OfferGuid = offerGuid;
		}
	}
}