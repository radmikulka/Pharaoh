// =========================================
// AUTHOR: Radek Mikulka
// DATE:   16.10.2023
// =========================================

using AldaEngine;
using Newtonsoft.Json;
using ServerData;

namespace ServerData.Dto
{
	public class CValuableModificationDto
	{
		[JsonProperty] public EModificationSource Source { get; set; }
		[JsonProperty] public CValuableDto Valuable { get; set; }
		[JsonProperty] public CValuableDto OwnedValue { get; set; }

		public CValuableModificationDto()
		{
		}

		public CValuableModificationDto(EModificationSource source, CValuableDto valuable, CValuableDto ownedValue)
		{
			Source = source;
			Valuable = valuable;
			OwnedValue = ownedValue;
		}
	}
}