// =========================================
// AUTHOR: Radek Mikulka
// DATE:   07.09.2023
// =========================================

using Newtonsoft.Json;

namespace ServerData.Hits
{
	public class CConfigureServerRequest : CRequestHit
	{
		[JsonProperty] public int FakeLatencyInSecs { get; set; }

		public CConfigureServerRequest() : base(EHit.ConfigureServerRequest)
		{
		}
		
		public CConfigureServerRequest(int fakeLatencyInSecs) : base(EHit.ConfigureServerRequest)
		{
			FakeLatencyInSecs = fakeLatencyInSecs;
		}
	}

	public class CConfigureServerResponse : CResponseHit
	{
		public CConfigureServerResponse() : base(EHit.ConfigureServerResponse)
		{
		}
	}
}