// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CBadGameVersionResponse : CResponseHit
	{
		[JsonProperty] public long ServerStartTime { get; set; }
		[JsonProperty] public long CurrentServerTime { get; set; }
		
		public CBadGameVersionResponse() : base(EHit.BadGameVersionResponse)
		{
		}

		public CBadGameVersionResponse(long serverStartTime, long currentServerTime) : base(EHit.BadGameVersionResponse)
		{
			ServerStartTime = serverStartTime;
			CurrentServerTime = currentServerTime;
		}
	}
	
	public class CDataServerUnreachableResponse : CResponseHit
	{
		public CDataServerUnreachableResponse() : base(EHit.DataServerUnreachable)
		{
		}
	}

	public class CInvalidPurchaseResponse : CResponseHit
	{
		public CInvalidPurchaseResponse() : base(EHit.InvalidPurchase)
		{
		}
	}
	
	public class CInvalidAuthResponse : CResponseHit
	{
		public CInvalidAuthResponse() : base(EHit.InvalidAuth)
		{
		}
	}
	
	public class CErrorResponse : CResponseHit
	{
		public CErrorResponse() : base(EHit.InternalError)
		{
		}
	}
	
	public class CSessionExpiredResponse : CResponseHit
	{
		public CSessionExpiredResponse() : base(EHit.SessionExpired)
		{
		}
	}
	
	public class CTooManyRequestsResponse : CResponseHit
	{
		public CTooManyRequestsResponse() : base(EHit.TooManyRequests)
		{
		}
	}
	
	public class CInvalidAppVersionResponse : CResponseHit
	{
		public CInvalidAppVersionResponse() : base(EHit.InvalidAppVersion)
		{
		}
	}
}