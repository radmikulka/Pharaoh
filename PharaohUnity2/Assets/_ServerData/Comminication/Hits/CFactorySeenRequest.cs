// =========================================
// AUTHOR: Radek Mikulka
// DATE:   03.11.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData.Hits
{
	public class CFactorySeenRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public EFactory Factory { get; set; }
		
		public CFactorySeenRequest() : base(EHit.FactorySeenRequest)
		{
		}
		
		public CFactorySeenRequest(EFactory factory) : base(EHit.FactorySeenRequest)
		{
			Factory = factory;
		}
	}
	
	public class CFactorySeenResponse : CResponseHit
	{
		public CFactorySeenResponse() : base(EHit.FactorySeenResponse)
		{
		}
	}
}