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
		[JsonProperty] public string PublicId { get; set; }
		[JsonProperty] public bool IsTestUser { get; set; }

		public CAccountDto()
		{
		}

		public CAccountDto(string publicId, bool isTestUser)
		{
			PublicId = publicId;
			IsTestUser = isTestUser;
		}
	}
}