// =========================================
// AUTHOR: Marek Karaba
// DATE:   12.01.2026
// =========================================

using System.Collections.Generic;
using System.Linq;
using AldaEngine;
using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	public class CSideCityGraphicProvider
	{
		private readonly CResourceConfigs _resourceConfigs;
		private readonly IBundleManager _bundleManager;

		public CSideCityGraphicProvider(CResourceConfigs resourceConfigs, IBundleManager bundleManager)
		{
			_resourceConfigs = resourceConfigs;
			_bundleManager = bundleManager;
		}

		public Sprite GetNametagSprite(ECity cityId)
		{
			ELiveEvent liveEventId = GetLiveEventOrDefault(cityId);
			bool isLiveEvent = liveEventId != ELiveEvent.None;
			
			Sprite nameTagSprite = isLiveEvent ? GetNameTagSpriteFromLiveEvent(liveEventId) : GetNametagFromSideCity(cityId);;
			return nameTagSprite;
		}
		
		public Sprite GetContractSprite(ECity cityId)
		{
			ELiveEvent liveEventId = GetLiveEventOrDefault(cityId);
			bool isLiveEvent = liveEventId != ELiveEvent.None;
			
			Sprite contractSprite = isLiveEvent ? GetContractSpriteFromLiveEvent(liveEventId) : GetContractSpriteFromSideCity(cityId);;
			return contractSprite;
		}

		private Sprite GetContractSpriteFromSideCity(ECity cityId)
		{
			CSideCityConfig config = _resourceConfigs.SideCities.GetConfig(cityId);
			Sprite contractSprite = _bundleManager.LoadItem<Sprite>(config.ContractImage);
			return contractSprite;
		}

		private Sprite GetContractSpriteFromLiveEvent(ELiveEvent liveEventId)
		{
			CLiveEventResourceConfig liveEventConfig = _resourceConfigs.LiveEvents.GetConfig(liveEventId);
			Sprite contractSprite = _bundleManager.LoadItem<Sprite>(liveEventConfig.ContractBanner);
			return contractSprite;
		}

		private Sprite GetNameTagSpriteFromLiveEvent(ELiveEvent liveEventId)
		{
			CLiveEventResourceConfig liveEventConfig = _resourceConfigs.LiveEvents.GetConfig(liveEventId);
			CLiveEventSideCityConfig sideCityConfig = liveEventConfig.GetSideCityConfig();
			Sprite nameTagSprite = _bundleManager.LoadItem<Sprite>(sideCityConfig.NametagSprite);
			return nameTagSprite;
		}
		
		private Sprite GetNametagFromSideCity(ECity cityId)
		{
			CSideCityConfig config = _resourceConfigs.SideCities.GetConfig(cityId);
			Sprite nameTagSprite = _bundleManager.LoadItem<Sprite>(config.NametagSprite);
			return nameTagSprite;
		}

		public ELiveEvent GetLiveEventOrDefault(ECity cityId)
		{
			IEnumerable<CLiveEventResourceConfig> configs = _resourceConfigs.LiveEvents.GetConfigs();
			foreach (CLiveEventResourceConfig config in configs)
			{
				if (!config.HasSideCity(cityId))
					continue;
				
				return config.Id;
			}
			return ELiveEvent.None;
		}
	}
}