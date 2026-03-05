// =========================================
// AUTHOR: Radek Mikulka
// DATE:   29.07.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData.Hits
{
	public class CAddXpRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public int XpToAdd { get; set; }
		
		public CAddXpRequest() : base(EHit.AddXpRequest)
		{
		}
		
		public CAddXpRequest(int xpToAdd) : base(EHit.AddXpRequest)
		{
			XpToAdd = xpToAdd;
		}
	}

	public class CAddXpResponse : CResponseHit
	{
		public CAddXpResponse() : base(EHit.AddXpResponse)
		{
		}
	}
}