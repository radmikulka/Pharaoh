// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.09.2025
// =========================================

namespace ServerData
{
	public class CValuableRequirement : IUpgradeRequirement
	{
		public readonly IValuable Valuable;

		public CValuableRequirement(IValuable valuable)
		{
			Valuable = valuable;
		}
	}
}