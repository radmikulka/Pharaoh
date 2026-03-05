// =========================================
// AUTHOR: Radek Mikulka
// DATE:   20.12.2023
// =========================================

using Newtonsoft.Json;

namespace ServerData.Hits
{
	public class CAdSeenRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public double Revenue { get; set; }
		
		public CAdSeenRequest() : base(EHit.AdSeenRequest)
		{
		}
	
		public CAdSeenRequest(double revenue) : base(EHit.AdSeenRequest)
		{
			Revenue = revenue;
		}
	}

	public class CAdSeenResponse : CResponseHit
	{
		public CAdSeenResponse() : base(EHit.AdSeenResponse)
		{
		}
	}
}

