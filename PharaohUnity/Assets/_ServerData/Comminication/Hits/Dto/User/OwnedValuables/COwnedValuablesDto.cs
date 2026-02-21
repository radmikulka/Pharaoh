// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.12.2024
// =========================================

using AldaEngine;
using Newtonsoft.Json;

namespace ServerData
{
	public class COwnedValuablesDto : IMapAble
	{
		[JsonProperty] public COwnedValuableDto[] Valuables { get; set; }

		public COwnedValuablesDto()
		{
		}

		public COwnedValuablesDto(COwnedValuableDto[] valuables)
		{
			Valuables = valuables;
		}
	}
}