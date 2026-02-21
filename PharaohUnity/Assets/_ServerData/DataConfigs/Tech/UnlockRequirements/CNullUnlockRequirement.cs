// =========================================
// AUTHOR: Radek Mikulka
// DATE:   01.10.2025
// =========================================

namespace ServerData
{
	public class CNullUnlockRequirement : IUnlockRequirement
	{
		public static readonly CNullUnlockRequirement Instance = new();

		public CNullUnlockRequirement()
		{
		}
	}
}