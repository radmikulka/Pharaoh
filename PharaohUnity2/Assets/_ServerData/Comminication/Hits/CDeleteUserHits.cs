// =========================================
// AUTHOR: Radek Mikulka
// DATE:   07.09.2023
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CDeleteUserRequest : CRequestHit
	{
		[JsonProperty] public CAuthDataRequestDto Auth { get; set; }

		public CDeleteUserRequest() : base(EHit.DeleteUserRequest)
		{
		}
		
		public CDeleteUserRequest(CAuthDataRequestDto auth) : base(EHit.DeleteUserRequest)
		{
			Auth = auth;
		}
	}

	public class CDeleteUserResponse : CResponseHit
	{
		public CDeleteUserResponse() : base(EHit.DeleteUserResponse)
		{
		}
	}
}

