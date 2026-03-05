// =========================================
// AUTHOR: Marek Karaba
// DATE:   11.12.2025
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class COpenEventOverviewTask
	{
		public readonly ELiveEvent LiveEventId;
		public readonly EEventOverviewTab Tab;

		public COpenEventOverviewTask(ELiveEvent liveEventId, EEventOverviewTab tab = EEventOverviewTab.None)
		{
			LiveEventId = liveEventId;
			Tab = tab;
		}
	}
}