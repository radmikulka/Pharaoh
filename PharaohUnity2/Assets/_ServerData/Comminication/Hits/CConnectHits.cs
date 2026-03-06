// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

using Newtonsoft.Json;
using ServerData;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CConnectRequest : CRequestHit
	{
		[JsonProperty] public CAuthDataRequestDto Auth { get; set; }
		[JsonProperty] public CDebugAuthDataDto DebugAuth { get; set; }
		[JsonProperty] public CDeviceDataDto DeviceData { get; set; }

		public CConnectRequest() : base(EHit.ConnectRequest)
		{
		}
		
		public CConnectRequest(
			CAuthDataRequestDto auth, 
			CDebugAuthDataDto debugAuth, 
			CDeviceDataDto deviceData
			) 
			: base(EHit.ConnectRequest)
		{
			Auth = auth;
			DebugAuth = debugAuth;
			DeviceData = deviceData;
		}
	}
	
	public class CConnectResponse : CResponseHit
	{
		[JsonProperty] public string AuthUid { get; set; }
		[JsonProperty] public string CommunicationToken { get; set; }
		[JsonProperty] public long DayRefreshTimeInMs { get; set; }
		[JsonProperty] public string DebugUserUid { get; set; }
		[JsonProperty] public long ServerTimeInMs { get; set; }
		[JsonProperty] public CUserDto User { get; set; }

		public CConnectResponse() : base(EHit.ConnectResponse)
		{
		}

		public CConnectResponse(
			CUserDto user, 
			string authUid, 
			long serverTimeInMs,
			long dayRefreshTimeInMs,
			string debugUserUid,
			string communicationToken
			) 
			: base(EHit.ConnectResponse)
		{
			DayRefreshTimeInMs = dayRefreshTimeInMs;
			CommunicationToken = communicationToken;
			ServerTimeInMs = serverTimeInMs;
			DebugUserUid = debugUserUid;
			AuthUid = authUid;
			User = user;
		}
	}

	public class CAccountDeletionPendingResponse : CResponseHit
	{
		[JsonProperty] public long TimeToDeleteAccountInMs { get; set; }

		public CAccountDeletionPendingResponse() : base(EHit.AccountDeletionPending)
		{
			
		}
		
		public CAccountDeletionPendingResponse(long timeToDeleteAccountInMs) : base(EHit.AccountDeletionPending)
		{
			TimeToDeleteAccountInMs = timeToDeleteAccountInMs;
		}
	}
}