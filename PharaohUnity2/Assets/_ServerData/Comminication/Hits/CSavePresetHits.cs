// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

using Newtonsoft.Json;

namespace ServerData.Hits
{
	public class CSavePresetRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public string PresetName { get; set; }
		
		public CSavePresetRequest() : base(EHit.SavePresetRequest)
		{
		}
		
		public CSavePresetRequest(string presetName) : base(EHit.SavePresetRequest)
		{
			PresetName = presetName;
		}
	}
	
	public class CSavePresetResponse : CResponseHit
	{
		public CSavePresetResponse() : base(EHit.SavePresetResponse)
		{
		}
	}
}