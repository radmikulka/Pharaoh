// =========================================
// AUTHOR: Marek Karaba
// DATE:   31.07.2025
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class CGainResourceParticleTask
	{
		public readonly CResourceValuable Resource;
		public readonly CValueModifyParams ModifyParams;

		public CGainResourceParticleTask(CResourceValuable resource, CValueModifyParams modifyParams)
		{
			Resource = resource;
			ModifyParams = modifyParams;
		}
	}
}