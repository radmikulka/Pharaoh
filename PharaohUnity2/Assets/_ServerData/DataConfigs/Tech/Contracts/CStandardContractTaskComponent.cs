// =========================================
// AUTHOR: Radek Mikulka
// DATE:   23.02.2026
// =========================================

namespace ServerData
{
	public class CStandardContractTaskComponent : IContractTaskComponent
	{
		public readonly SResource Resource;

		public CStandardContractTaskComponent(SResource resource)
		{
			Resource = resource;
		}
	}
}
