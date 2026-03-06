// =========================================
// AUTHOR: Radek Mikulka
// DATE:   16.1.2024
// =========================================

using Newtonsoft.Json;

namespace ServerData
{
	public abstract class COwnedValuableData : IOwnedValuableData
	{
		[JsonProperty] public EValuable Id { get; set; }

		protected COwnedValuableData()
		{
		}

		protected COwnedValuableData(EValuable id)
		{
			Id = id;
		}
	}
}

