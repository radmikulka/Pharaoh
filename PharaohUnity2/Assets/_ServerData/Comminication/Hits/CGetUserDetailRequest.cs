// =========================================
// AUTHOR: Radek Mikulka
// DATE:   22.01.2026
// =========================================

using AldaEngine;
using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CGetUserDetailRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public string EncryptedUid { get; set; }
		
		public CGetUserDetailRequest() : base(EHit.GetUserDetailRequest)
		{
		}
		
		public CGetUserDetailRequest(string encryptedUid) : base(EHit.GetUserDetailRequest)
		{
			EncryptedUid = encryptedUid;
		}
	}

	public class CGetUserDetailResponse : CResponseHit
	{
		[JsonProperty] public CUserSocialDto Social { get; private set; }
		[JsonProperty] public EYearMilestone Year { get; private set; }
		[JsonProperty] public ECountryCode Country { get; private set; }
		[JsonProperty] public EPlatform Platform { get; private set; }
		[JsonProperty] public int TotalVehicles { get; private set; }
		[JsonProperty] public int FinishedContracts { get; private set; }
		[JsonProperty] public int TotalDispatchers { get; private set; }
	
		public CGetUserDetailResponse() : base(EHit.GetUserDetailResponse)
		{
		}

		public CGetUserDetailResponse(
			CUserSocialDto social, 
			EYearMilestone year, 
			ECountryCode country, 
			EPlatform platform, 
			int totalVehicles, 
			int finishedContracts, 
			int totalDispatchers
			) : base(EHit.GetUserDetailResponse)
		{
			Social = social;
			Year = year;
			Country = country;
			Platform = platform;
			TotalVehicles = totalVehicles;
			FinishedContracts = finishedContracts;
			TotalDispatchers = totalDispatchers;
		}
	}
}