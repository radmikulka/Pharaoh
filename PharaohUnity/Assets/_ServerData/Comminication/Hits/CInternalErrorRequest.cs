// =========================================
// AUTHOR: Radek Mikulka
// DATE:   07.12.2023
// =========================================

using Newtonsoft.Json;

namespace ServerData.Hits
{
	public class CInternalErrorRequest : CCommTokenBasedRequest
	{
		public CInternalErrorRequest() : base(EHit.InternalErrorRequest)
		{
		}
	}
}