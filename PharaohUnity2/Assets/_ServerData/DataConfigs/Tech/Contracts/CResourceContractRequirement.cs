// =========================================
// AUTHOR: Radek Mikulka
// DATE:   24.02.2026
// =========================================

namespace ServerData
{
	public class CResourceContractRequirement : IContractRequirement
	{
		public readonly SResource Resource;

		public CResourceContractRequirement(SResource resource)
		{
			Resource = resource;
		}
	}
}
