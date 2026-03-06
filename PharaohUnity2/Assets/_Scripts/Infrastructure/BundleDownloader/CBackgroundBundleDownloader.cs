// =========================================
// AUTHOR: Radek Mikulka
// DATE:   11.11.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using ServerData;
using ServerData.Design;
using TycoonBuilder.Offers;
using Zenject;

namespace TycoonBuilder
{
	public class CBackgroundBundleDownloader : IInitializable
	{
		private readonly IRequiredBundlesProvider _requiredBundlesProvider;
		private readonly CDesignVehicleConfigs _vehicleConfigs;
		private readonly IBundleManager _bundleManager;
		private readonly ICtsProvider _ctsProvider;
		private readonly IEventBus _eventBus;
		private readonly CUser _user;

		public CBackgroundBundleDownloader(
			IRequiredBundlesProvider requiredBundlesProvider, 
			CDesignVehicleConfigs vehicleConfigs, 
			IBundleManager bundleManager, 
			ICtsProvider ctsProvider, 
			IEventBus eventBus, 
			CUser user
			)
		{
			_requiredBundlesProvider = requiredBundlesProvider;
			_vehicleConfigs = vehicleConfigs;
			_bundleManager = bundleManager;
			_ctsProvider = ctsProvider;
			_eventBus = eventBus;
			_user = user;
		}

		public void Initialize()
		{
			DownloadAndLoadBundles(new []{(int)EBundleId.Region1Environment_2}, _ctsProvider.Token).Forget();

			_eventBus.Subscribe<CNewOffersSyncedSignal>(OnNewOffersSynced);
			_eventBus.Subscribe<CYearSeenSignal>(OnYearSeen);
		}

		private void OnYearSeen(CYearSeenSignal signal)
		{
			int[] requiredBundles = _requiredBundlesProvider.GetBundles(_user.Progress.Mission);
			_bundleManager.LoadBundlesAsync(requiredBundles, _ctsProvider.Token).Forget();
		}

		private void OnNewOffersSynced(CNewOffersSyncedSignal signal)
		{
			LoadOfferBundles(_ctsProvider.Token).Forget();
			return;
			
			async UniTaskVoid LoadOfferBundles(CancellationToken ct)
			{
				bool newBundlesLoaded = await TryDownloadMissingBundles(signal.NewOffers, ct);
				if(!newBundlesLoaded)
					return;
				_eventBus.Send(new COfferBundlesDownloadedSignal());
			}
		}
		
		private async UniTask<bool> TryDownloadMissingBundles(COffer[] newOffers, CancellationToken ct)
		{
			HashSet<EVehicle> vehiclesToDownload = null;
			foreach (COffer offer in newOffers)
			{
				foreach (IValuable reward in offer.Rewards)
				{
					if(reward is not CVehicleValuable vehicleValuable)
						continue;
					vehiclesToDownload ??= new HashSet<EVehicle>();
					vehiclesToDownload.Add(vehicleValuable.Vehicle);
				}
			}

			if (vehiclesToDownload == null)
				return false;

			HashSet<int> bundlesToLoad = null;
			foreach (EVehicle vehicle in vehiclesToDownload)
			{
				CVehicleConfig vehicleConfig = _vehicleConfigs.GetConfig(vehicle);
				if(vehicleConfig.OverrideBundleId == EBundleId.None)
					continue;
					
				bool isLoaded = _bundleManager.IsBundleLoaded((int)vehicleConfig.OverrideBundleId);
				if (isLoaded)
					continue;
					
				bundlesToLoad ??= new HashSet<int>();
				bundlesToLoad.Add((int)vehicleConfig.OverrideBundleId);
			}

			if (bundlesToLoad == null)
				return false;

			await _bundleManager.LoadBundlesAsync(bundlesToLoad.ToArray(), ct);
			return true;
		}

		private async UniTask DownloadAndLoadBundles(int[] bundles, CancellationToken ct)
		{
			bool completed = _bundleManager.AreBundlesLoaded(bundles);
			if(completed)
				return;
			
			while (true)
			{
				bool result = await _bundleManager.LoadBundlesAsync(bundles, ct);
				if (result)
				{
					return;
				}
				await UniTask.WaitForSeconds(10f, cancellationToken: ct);
			}
		}
	}
}