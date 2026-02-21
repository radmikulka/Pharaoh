// =========================================
// AUTHOR: Radek Mikulka
// DATE:   30.05.2024
// =========================================

using ServerData;

namespace ServerData
{
	public interface IUnlockRequirement
	{
		private static readonly CNullUnlockRequirement NullUnlockRequirement = new();

		public static CNullUnlockRequirement Null() => NullUnlockRequirement;

		
		public static CCompositeUnlockRequirement Composite(params IUnlockRequirement[] requirements)
		{
			return new CCompositeUnlockRequirement(requirements);
		}
	}
}