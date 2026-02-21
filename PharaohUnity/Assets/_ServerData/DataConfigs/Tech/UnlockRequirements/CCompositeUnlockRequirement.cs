// =========================================
// AUTHOR: Radek Mikulka
// DATE:   01.10.2025
// =========================================

namespace ServerData
{
	public class CCompositeUnlockRequirement : IUnlockRequirement
	{
		public readonly IUnlockRequirement[] Requirements;

		public CCompositeUnlockRequirement(params IUnlockRequirement[] requirements)
		{
			Requirements = requirements;
		}
	}
}