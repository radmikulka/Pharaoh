// =========================================
// AUTHOR: Radek Mikulka
// DATE:   16.1.2024
// =========================================

using Newtonsoft.Json;

namespace ServerData.Hits
{
	public abstract class CCommTokenBasedRequest : CRequestHit
	{
		[JsonProperty] public string CommunicationToken { get; set; }
		[JsonProperty] public long ClientTime { get; set; }
		
		protected CCommTokenBasedRequest(EHit hitType) : base(hitType)
		{
			
		}
	}
}

