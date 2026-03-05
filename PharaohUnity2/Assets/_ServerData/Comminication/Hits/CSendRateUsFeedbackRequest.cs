// =========================================
// AUTHOR: Radek Mikulka
// DATE:   29.07.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData.Hits
{
	public class CSendRateUsFeedbackRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public string Content { get; set; }
		
		public CSendRateUsFeedbackRequest() : base(EHit.SendRateUsFeedbackRequest)
		{
		}
		
		public CSendRateUsFeedbackRequest(string content) : base(EHit.SendRateUsFeedbackRequest)
		{
			Content = content;
		}
	}

	public class CSendRateUsFeedbackResponse : CResponseHit
	{
		public CSendRateUsFeedbackResponse() : base(EHit.SendRateUsFeedbackResponse)
		{
		}
	}
}