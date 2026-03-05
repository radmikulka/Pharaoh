// =========================================
// AUTHOR: Radek Mikulka
// DATE:   29.11.2023
// =========================================

using AldaEngine;
using Newtonsoft.Json;

namespace ServerData.Dto
{
	public class CAuthDataResponseDto
	{
		[JsonProperty] public string UserAuthUid { get; set; }
		[JsonProperty] public EAuthType[] LoggedServices { get; set; }

		public CAuthDataResponseDto()
		{
		}

		public CAuthDataResponseDto(string userAuthUid, EAuthType[] loggedServices)
		{
			UserAuthUid = userAuthUid;
			LoggedServices = loggedServices;
		}
	}
}