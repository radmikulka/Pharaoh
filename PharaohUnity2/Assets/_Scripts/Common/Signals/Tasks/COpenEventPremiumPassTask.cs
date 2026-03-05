// =========================================
// AUTHOR: Marek Karaba
// DATE:   08.01.2026
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class COpenEventPremiumPassTask
	{
		public readonly ELiveEvent LiveEventId;

		public COpenEventPremiumPassTask(ELiveEvent liveEventId)
		{
			LiveEventId = liveEventId;
		}
	}
}