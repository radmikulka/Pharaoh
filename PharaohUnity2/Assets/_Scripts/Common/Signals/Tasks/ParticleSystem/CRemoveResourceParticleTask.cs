// =========================================
// AUTHOR: Marek Karaba
// DATE:   31.07.2025
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class CRemoveResourceParticleTask
	{
		public readonly CResourceValuable Resource;

		public CRemoveResourceParticleTask(CResourceValuable resource)
		{
			Resource = resource;
		}
	}
}