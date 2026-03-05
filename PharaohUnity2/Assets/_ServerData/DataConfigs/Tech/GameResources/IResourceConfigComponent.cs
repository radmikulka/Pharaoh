// =========================================
// AUTHOR: Radek Mikulka
// DATE:   11.09.2025
// =========================================

namespace ServerData
{
	public interface IResourceConfigComponent
	{
		public static CConsumableResourceData Consumable(IValuable[] rewards) => new(rewards);
	}
}