// =========================================
// AUTHOR: Radek Mikulka
// DATE:   11.11.2025
// =========================================

using System.Collections.Generic;
using System.Linq;
using AldaEngine;
using ServerData;
using ServerData.Design;

namespace TycoonBuilder
{
	public class CRequiredBundlesProvider : IRequiredBundlesProvider
	{
		private readonly CLiveEventBundles _liveEventBundles;
		private readonly CDesignRegionConfigs _regionConfigs;
		private readonly CResourceConfigs _resourceConfigs;
		private readonly CUser _user;

		public CRequiredBundlesProvider(CDesignRegionConfigs regionConfigs, CUser user, CLiveEventBundles liveEventBundles, CResourceConfigs resourceConfigs)
		{
			_liveEventBundles = liveEventBundles;
			_resourceConfigs = resourceConfigs;
			_regionConfigs = regionConfigs;
			_user = user;
		}

		public int[] GetBundles(ERegion region)
		{
			HashSet<EBundleId> bundles = new();
			GetBaseBundles(bundles);
			bundles.UnionWith(_user.Offers.GetSpecialOfferBundles());
			AddRegionBundles(bundles);
			AddContentRegionBundles(region, bundles);
			AddEnviroRegionBundles(region, bundles);
			AddEventBundles(bundles);
			AddOwnedVehicles(bundles);
			
			if (_user.Progress.Year >= EYearMilestone._1931)
			{
				bundles.Add(EBundleId.Region1Environment_2);
			}

			return bundles.Select(id => (int)id).ToArray();
		}

		private void AddOwnedVehicles(HashSet<EBundleId> target)
		{
			foreach (COwnedVehicle vehicle in _user.Vehicles.GetAllOwnedVehicles())
			{
				if(vehicle.Config.OverrideBundleId == EBundleId.None)
					continue;
				target.Add(vehicle.Config.OverrideBundleId);
			}
		}

		private void AddEventBundles(HashSet<EBundleId> target)
		{
			IEnumerable<ELiveEvent> activeEvent = _user.LiveEvents.GetActiveEvents();
			foreach (ELiveEvent liveEvent in activeEvent)
			{
				CLiveEventBundle eventConfig = _liveEventBundles.GetConfig(liveEvent);
				target.Add(eventConfig.ContentBundle);
				target.Add(eventConfig.UiBundle);
			}
			
			IEnumerable<ELiveEvent> requiredEvents = _user.GetRequiredEvents();
			foreach (ELiveEvent liveEvent in requiredEvents)
			{
				CLiveEventBundle eventConfig = _liveEventBundles.GetConfig(liveEvent);
				target.Add(eventConfig.ContentBundle);
			}
			
			EYearMilestone year = _user.Progress.Year;
			if(CDesignGlobalConfig.LiveEventUnlockYear - 1 > year)
				return;
			
			foreach (ELiveEvent liveEvent in _user.LiveEvents.FutureEventIds)
			{
				CLiveEventBundle eventConfig = _liveEventBundles.GetConfig(liveEvent);
				target.Add(eventConfig.ContentBundle);
				target.Add(eventConfig.UiBundle);
			}
		}

		private void AddRegionBundles(HashSet<EBundleId> target)
		{
			foreach (CRegionConfig region in _regionConfigs.OrderedRegions)
			{
				AddContentRegionBundles(region.Id, target);
				
				if(region.Id == _user.Progress.Region)
					return;
			}
		}
		
		private void AddContentRegionBundles(ERegion region, HashSet<EBundleId> target)
		{
			CRegionConfig regionConfig = _regionConfigs.GetRegionConfig(region);
			target.Add(regionConfig.ContentBundleId);
		}

		private void AddEnviroRegionBundles(ERegion region, HashSet<EBundleId> target)
		{
			CRegionConfig regionConfig = _regionConfigs.GetRegionConfig(region);
			target.Add(regionConfig.EnviroBundle);
			
			CRegionResourceConfig resourceConfig = _resourceConfigs.Regions.GetConfig(region);
			CSceneResourceConfig sceneConfig = _resourceConfigs.Scenes.GetConfig(resourceConfig.MainScene);

			foreach (EBundleId bundleId in sceneConfig.BundleIds)
			{
				target.Add(bundleId);
			}
		}

		private void GetBaseBundles(HashSet<EBundleId> target)
		{
			target.Add(EBundleId.CoreGame);
			target.Add(EBundleId.CoreGameScenes);
			target.Add(EBundleId.BaseGame);
			target.Add(EBundleId.BaseGameScene);
			target.Add(EBundleId.VehicleDetailBase);
			target.Add(EBundleId.VehicleDetailScene);
			target.Add(EBundleId.Region1_Content);
			target.Add(EBundleId.Region1Environment_1);
			target.Add(EBundleId.Region1Scene);
		}
	}
}