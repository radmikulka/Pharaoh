// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.12.2025
// =========================================

using System.Collections.Generic;

namespace ServerData
{
	public class CLiveEventBundles
	{
		private readonly Dictionary<ELiveEvent, CLiveEventBundle> _configs = new();

		public CLiveEventBundles()
		{
			InitConfigs();
		}

		public CLiveEventBundle GetConfig(ELiveEvent id)
		{
			return _configs[id];
		}

		private void InitConfigs()
		{
			AddConfig(ELiveEvent.EarthAndFire, EBundleId.EarthAndFireEventContent, EBundleId.LiveEventsQ1);
			AddConfig(ELiveEvent.BankingTycoon, EBundleId.EarthAndFireEventContent, EBundleId.LiveEventsQ1);
		}

		private void AddConfig(ELiveEvent liveEvent, EBundleId contentBundle, EBundleId uiBundle)
		{
			CLiveEventBundle config = new(liveEvent, contentBundle, uiBundle);
			_configs.Add(liveEvent, config);
		}
	}
}