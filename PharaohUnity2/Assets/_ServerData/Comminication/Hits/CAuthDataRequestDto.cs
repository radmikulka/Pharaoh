// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

using AldaEngine;
using Newtonsoft.Json;

namespace ServerData.Dto
{
	public class CAuthDataRequestDto
	{
		[JsonProperty] public string UserAuthUid { get; set; }
		[JsonProperty] public string DeviceId { get; set; }

		public CAuthDataRequestDto()
		{
		}

		public CAuthDataRequestDto(string userAuthUid, string deviceId)
		{
			UserAuthUid = userAuthUid;
			DeviceId = deviceId;
		}
	}
}

