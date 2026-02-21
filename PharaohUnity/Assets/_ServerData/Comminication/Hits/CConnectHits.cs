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
		[JsonProperty] public string DeviceId { get; set; }
		[JsonProperty] public EUserPresetId PresetId { get; set; }

		public CConnectRequest() : base(EHit.ConnectRequest)
		{
		}
		
		public CConnectRequest(string deviceId, EUserPresetId presetId) : base(EHit.ConnectRequest)
		{
			DeviceId = deviceId;
			PresetId = presetId;
		}
	}
	
	public class CConnectResponse : CResponseHit
	{
		[JsonProperty] public CUserDto User { get; set; }

		public CConnectResponse() : base(EHit.ConnectResponse)
		{
		}

		public CConnectResponse(CUserDto user) : base(EHit.ConnectResponse)
		{
			User = user;
		}
	}
}