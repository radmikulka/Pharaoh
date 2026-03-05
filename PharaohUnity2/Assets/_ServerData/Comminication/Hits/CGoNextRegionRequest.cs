// =========================================
// AUTHOR: Radek Mikulka
// DATE:   16.12.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData.Hits
{
	public class CGoNextRegionRequest : CCommTokenBasedRequest
	{
		public CGoNextRegionRequest() : base(EHit.GoNextRegionRequest)
		{
		}
	}
	
	public class CGoNextRegionResponse : CResponseHit
	{
		public CGoNextRegionResponse() : base(EHit.GoNextRegionResponse)
		{
		}
	}
}