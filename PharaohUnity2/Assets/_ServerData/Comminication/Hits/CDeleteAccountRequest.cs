// =========================================
// AUTHOR: Radek Mikulka
// DATE:   15.08.2024
// =========================================

namespace ServerData.Hits
{
	public class CDeleteAccountRequest : CCommTokenBasedRequest
	{
		public CDeleteAccountRequest() : base(EHit.DeleteAccountRequest)
		{
		}
	}

	public class CDeleteAccountResponse : CResponseHit
	{
		public CDeleteAccountResponse() : base(EHit.DeleteAccountResponse)
		{
		}
	}
}