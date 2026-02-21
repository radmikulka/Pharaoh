// =========================================
// AUTHOR: Radek Mikulka
// DATE:   14.07.2025
// =========================================

using AldaEngine;
using Newtonsoft.Json;

namespace ServerData.Dto
{
	public class CAccountDto
	{
		[JsonProperty] public string Uid { get; set; }

		public CAccountDto()
		{
		}

		public CAccountDto(string uid)
		{
			Uid = uid;
		}
	}
}