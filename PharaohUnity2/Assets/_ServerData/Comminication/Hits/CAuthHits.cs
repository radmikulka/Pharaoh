// =========================================
// AUTHOR: Radek Mikulka
// DATE:   13.11.2023
// =========================================

using AldaEngine;
using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CAuthInfoRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public EAuthType OtherServiceAuthType { get; set; }
		[JsonProperty] public string OtherServiceToken { get; set; }
		[JsonProperty] public string UserAuthUid { get; set; }
		[JsonProperty] public string ServiceId { get; set; }
		
		public CAuthInfoRequest() : base(EHit.AuthInfoRequest)
		{
		}

		public CAuthInfoRequest(
			EAuthType otherServiceAuthType, 
			string otherServiceToken, 
			string userAuthUid,
			string serviceId
			) 
			: base(EHit.AuthInfoRequest)
		{
			UserAuthUid = userAuthUid;
			ServiceId = serviceId;
			OtherServiceToken = otherServiceToken;
			OtherServiceAuthType = otherServiceAuthType;
		}
	}
	
	public class CAuthInfoResponse : CResponseHit
	{
		[JsonProperty] public CProfileConflictingUserDto ConflictingUser { get; set; }
		
		public CAuthInfoResponse() : base(EHit.AuthInfoResponse)
		{
		}
		
		public CAuthInfoResponse(CProfileConflictingUserDto conflictingUser) : base(EHit.AuthInfoResponse)
		{
			ConflictingUser = conflictingUser;
		}
	}
	
	public class CLinkAccountRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public EAuthType OtherServiceAuthType { get; set; }
		[JsonProperty] public string OtherServiceToken { get; set; }
		[JsonProperty] public string UserAuthUid { get; set; }
		[JsonProperty] public string ServiceId { get; set; }
		[JsonProperty] public bool UseRemoteUser { get; set; }
		
		public CLinkAccountRequest() : base(EHit.LinkAccountRequest)
		{
		}
		
		public CLinkAccountRequest(
			string userAuthUid, 
			EAuthType otherServiceAuthType, 
			string otherServiceToken, 
			string serviceId, 
			bool useRemoteUser) 
			: base(EHit.LinkAccountRequest)
		{
			UseRemoteUser = useRemoteUser;
			OtherServiceAuthType = otherServiceAuthType;
			OtherServiceToken = otherServiceToken;
			UserAuthUid = userAuthUid;
			ServiceId = serviceId;
		}
	}
	
	public class CLinkAccountResponse : CResponseHit
	{
		[JsonProperty] public CAuthDataResponseDto NewAuth { get; set; }
		
		public CLinkAccountResponse() : base(EHit.LinkAccountResponse)
		{
		}
		
		public CLinkAccountResponse(CAuthDataResponseDto newAuth) : base(EHit.LinkAccountResponse)
		{
			NewAuth = newAuth;
		}
	}
}