// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.12.2024
// =========================================

using AldaEngine;
using Newtonsoft.Json;

namespace ServerData
{
	public class COwnedValuableDto : IJsonMap
	{
		[JsonProperty] public string Json { get; set; }

		public COwnedValuableDto()
		{
		}
	}
}