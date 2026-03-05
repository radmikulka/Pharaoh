// =========================================
// AUTHOR: Radek Mikulka
// DATE:   04.09.2025
// =========================================

using AldaEngine;
using Newtonsoft.Json;

namespace ServerData.Dto
{
	public class CFactorySlotDto : IMapAble
	{
		[JsonProperty] public SResource CraftingProduct { get; set; }
		[JsonProperty] public long? CompletionTime { get; set; }
		[JsonProperty] public bool IsUnlocked { get; set; }
		[JsonProperty] public int Index { get; set; }

		public CFactorySlotDto()
		{
		}

		public CFactorySlotDto(SResource craftingProduct, long? completionTime, bool isUnlocked, int index)
		{
			CraftingProduct = craftingProduct;
			CompletionTime = completionTime;
			IsUnlocked = isUnlocked;
			Index = index;
		}
	}
}