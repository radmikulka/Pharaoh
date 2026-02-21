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
		[JsonProperty] public string DeviceId { get; set; }

		public CDeleteUserRequest() : base(EHit.DeleteUserRequest)
		{
		}
		
		public CDeleteUserRequest(string deviceId) : base(EHit.DeleteUserRequest)
		{
			DeviceId = deviceId;
		}
	}

	public class CDeleteUserResponse : CResponseHit
	{
		public CDeleteUserResponse() : base(EHit.DeleteUserResponse)
		{
		}
	}
}

