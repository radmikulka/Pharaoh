// =========================================
// NAME: Marek Karaba
// DATE: 12.02.2026
// =========================================

using ServerData;
using ServerData.Design;

namespace TycoonBuilder
{
	public class CIndustryRegionsProvider
	{
		private readonly CDesignIndustryConfigs _industryConfigs;
		private readonly CUser _user;

		public CIndustryRegionsProvider(CDesignIndustryConfigs industryConfigs, CUser user)
		{
			_industryConfigs = industryConfigs;
			_user = user;
		}

		public ERegion[] GetAvailableRegions(EResource resource)
		{
			CResourceIndustryConfig industryConfig = _industryConfigs.GetConfig(resource);
			return industryConfig.LiveEvent ==
			       ELiveEvent.None ? new [] { industryConfig.Region } : GetCurrentAndLastRegion();
		}

		private ERegion[] GetCurrentAndLastRegion()
		{
			ERegion currentRegion = _user.Progress.Region;
			if (currentRegion == ERegion.Region1)
			{
				return new[] { currentRegion };
			}

			ERegion previousRegion = (ERegion)((int)currentRegion - 1);
			return new[] { previousRegion, currentRegion };
		}
	}
}