// =========================================
// AUTHOR: Juraj Joscak
// DATE:   14.08.2025
// =========================================

namespace TycoonBuilder
{
	public class CShowPopUpOfferTask
	{
		public string OfferId { get; }
		public bool IsGroup { get; }
		
		public CShowPopUpOfferTask(string offerId, bool isGroup)
		{
			OfferId = offerId;
			IsGroup = isGroup;
		}
	}
}