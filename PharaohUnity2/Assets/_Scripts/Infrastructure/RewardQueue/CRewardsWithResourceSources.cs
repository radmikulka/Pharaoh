// =========================================
// AUTHOR: Marek Karaba
// DATE:   01.08.2025
// =========================================

using ServerData;

namespace TycoonBuilder.Infrastructure
{
	public class CRewardsWithResourceSources
	{
		public readonly IParticleSource[] ParticleSources;
		public readonly EModificationSource Source;
		public readonly IValuable[] Rewards;
		public readonly CValueModifyParams ModifyParams;

		public CRewardsWithResourceSources(IValuable[] rewards, IParticleSource[] particleSources, EModificationSource source, CValueModifyParams modifyParams)
		{
			ParticleSources = particleSources;
			Rewards = rewards;
			Source = source;
			ModifyParams = modifyParams;
		}
	}
}