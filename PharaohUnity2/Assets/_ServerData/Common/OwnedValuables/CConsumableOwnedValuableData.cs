// =========================================
// AUTHOR: Radek Mikulka
// DATE:   16.1.2024
// =========================================

using Newtonsoft.Json;

namespace ServerData
{
	public class CConsumableOwnedValuableData : COwnedValuableData
	{
		[JsonProperty] public int Amount { get; set; }

		public CConsumableOwnedValuableData(EValuable id, int amount) : base(id)
		{
			Amount = amount;
		}

		public CConsumableOwnedValuableData()
		{
		}
	}
}