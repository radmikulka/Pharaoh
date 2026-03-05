// =========================================
// AUTHOR: Juraj Joscak
// DATE:   25.11.2025
// =========================================

namespace ServerData
{
	public class COwnedValuableUnlockRequirement : IUnlockRequirement
	{
		public readonly IValuable Valuable;

		public COwnedValuableUnlockRequirement(IValuable valuable)
		{
			Valuable = valuable;
		}
	}
}