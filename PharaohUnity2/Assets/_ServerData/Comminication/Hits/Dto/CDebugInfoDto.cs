// =========================================
// AUTHOR: Radek Mikulka
// DATE:   29.07.2024
// =========================================

using AldaEngine;
using Newtonsoft.Json;

namespace ServerData.Dto
{
	public class CDebugInfoDto
	{
		#if DEBUG_MODE || !UNITY_ENGINE
		[JsonProperty] public int Sps { get; private set; }
		[JsonProperty] public string ContextualPrice { get; private set; }
		[JsonProperty] public ECountryCode Country { get; private set; }

		public CDebugInfoDto()
		{
		}

		public CDebugInfoDto(string contextualPrice, ECountryCode country, int sps)
		{
			ContextualPrice = contextualPrice;
			Country = country;
			Sps = sps;
		}
		#endif
	}
}