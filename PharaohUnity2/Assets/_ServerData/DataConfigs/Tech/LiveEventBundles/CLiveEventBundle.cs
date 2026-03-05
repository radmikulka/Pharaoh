// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.12.2025
// =========================================

namespace ServerData
{
	public class CLiveEventBundle
	{
		public readonly ELiveEvent LiveEvent;
		public readonly EBundleId ContentBundle;
		public readonly EBundleId UiBundle;

		public CLiveEventBundle(ELiveEvent liveEvent, EBundleId contentBundle, EBundleId uiBundle)
		{
			LiveEvent = liveEvent;
			ContentBundle = contentBundle;
			UiBundle = uiBundle;
		}
	}
}