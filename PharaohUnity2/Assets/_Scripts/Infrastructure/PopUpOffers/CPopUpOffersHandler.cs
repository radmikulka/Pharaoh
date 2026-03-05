// =========================================
// AUTHOR: Juraj Joscak
// DATE:   15.08.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using Server;
using ServerData;
using ServerData.Design;
using TycoonBuilder.Offers;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CPopUpOffersHandler : IInitializable
	{
		private readonly CDesignVehicleConfigs _vehicleConfigs;
		private readonly CLazyActionQueue _lazyActionQueue;
		private readonly IBundleManager _bundleManager;
		private readonly ICtsProvider _ctsProvider;
		private readonly IServerTime _serverTime;
		private readonly IEventBus _eventBus;
		private readonly CUser _user;
		private readonly CImageDownloader _imageDownloader;

		private readonly Dictionary<string, long> _lastPopUpTimes = new();

		private readonly EOfferType[] _groupTypes = new[]
		{
			EOfferType.MultiPack,
			EOfferType.PiggyBank
		};
		
		public CPopUpOffersHandler(
			CDesignVehicleConfigs vehicleConfigs, 
			CLazyActionQueue lazyActionQueue, 
			IBundleManager bundleManager, 
			ICtsProvider ctsProvider, 
			IServerTime serverTime, 
			IEventBus eventBus, 
			CUser user,
			CImageDownloader imageDownloader
			)
		{
			_lazyActionQueue = lazyActionQueue;
			_vehicleConfigs = vehicleConfigs;
			_bundleManager = bundleManager;
			_ctsProvider = ctsProvider;
			_serverTime = serverTime;
			_eventBus = eventBus;
			_user = user;
			_imageDownloader = imageDownloader;
		}
		
		public void Initialize()
		{
			LoadLastPopUpTimes();
			ClearOldPopUpTimes();
			SaveLastPopUpTimes();
			
			_eventBus.Subscribe<CCoreGameLoadedSignal>(ShowPopUpOffers);
			_eventBus.Subscribe<CNewOffersSyncedSignal>(ShowPopUpOffers);
			_eventBus.Subscribe<COfferBundlesDownloadedSignal>(ShowPopUpOffers);
		}
		
		private void ShowPopUpOffers(IEventBusSignal _)
		{
			ShowPopUpOffersAsync(_ctsProvider.Token).Forget();
		}
		
		private async UniTask ShowPopUpOffersAsync(CancellationToken ct)
		{
			await UniTask.WaitForSeconds(0.5f, cancellationToken: ct);
			
			if (CDebugConfig.Instance.ShouldSkip(EEditorSkips.Popups))
				return;
			
			if(!_user.Tutorials.IntroCompleted)
				return;
			
			COffer[] offers = _user.Offers.GetOffersWithParam(EOfferParam.OfferPlacement);
			for(int i = 0; i < offers.Length; i++)
			{
				if (ct.IsCancellationRequested)
					return;

				COffer offer = offers[i];
				if (!IsReadyForPopUp(offer))
					continue;
				
				_lazyActionQueue.AddAction(new CPopUpOfferAction((cancellationToken) => TryPopUpOffer(offer, cancellationToken)));
			}
			
			IEnumerable<COfferGroup> groups = _user.Offers.GetGroupsWithParam(EOfferParam.OfferPlacement);
			foreach (COfferGroup group in groups)
			{
				if (ct.IsCancellationRequested)
					return;

				if (!IsReadyForPopUp(group))
					continue;
				
				_lazyActionQueue.AddAction(new CPopUpOfferAction((cancellationToken) => TryPopUpGroup(group, cancellationToken)));
			}
		}
		
		private async UniTask TryPopUpOffer(COffer offer, CancellationToken ct)
		{
			if (!IsReadyForPopUp(offer))
				return;
			
			UpdateLastPopUpTime(offer.Guid);
			await _eventBus.ProcessTaskAsync(new CShowPopUpOfferTask(offer.Guid, false), ct);
		}
		
		private async UniTask TryPopUpGroup(COfferGroup group, CancellationToken ct)
		{
			if (!IsReadyForPopUp(group))
				return;
			
			UpdateLastPopUpTime(group.GroupId);
			await _eventBus.ProcessTaskAsync(new CShowPopUpOfferTask(group.GroupId, true), ct);
		}

		private bool IsReadyForPopUp(COffer offer)
		{
			if (!_user.Offers.IsOfferValid(offer))
				return false;
			
			EOfferPlacement placement = offer.GetParamValue<EOfferPlacement>(EOfferParam.OfferPlacement);
			if(!placement.HasFlag(EOfferPlacement.Popup))
				return false;

			if(!IsTimeToPopUp(offer.Guid, offer.Params))
				return false;
			
			if(offer.IsExpired(_serverTime.GetTimestampInMs()))
				return false;
			
			if(offer.MaxPurchasesReached())
				return false;

			string groupId = offer.GetParamValueOrDefault<string>(EOfferParam.GroupId);
			if (!string.IsNullOrEmpty(groupId))
			{
				COfferGroup group = _user.Offers.GetOfferGroup(groupId);
				if(_groupTypes.Contains(group.GetOfferType()))
					return false;
			}
			
			if (_groupTypes.Contains(offer.GetOfferType()))
				return false;
			
			string backgroundImage = offer.GetParamValueOrDefault<string>(EOfferParam.BackgroundImage);
			if (!string.IsNullOrEmpty(backgroundImage) && !_imageDownloader.IsDownloaded(backgroundImage))
				return false;
			
			bool bundlesLoaded = BundlesLoaded(offer);
			if(!bundlesLoaded)
				return false;
			
			return true;
		}

		private bool BundlesLoaded(COffer offer)
		{
			foreach (IValuable reward in offer.Rewards)
			{
				if(reward is not CVehicleValuable vehicleValuable)
					continue;

				CVehicleConfig vehicleConfig = _vehicleConfigs.GetConfig(vehicleValuable.Vehicle);
				if(vehicleConfig.OverrideBundleId == EBundleId.None)
					continue;
				
				bool isBundleLoaded = _bundleManager.IsBundleLoaded((int)vehicleConfig.OverrideBundleId);
				if (!isBundleLoaded)
				{
					return false;
				}
			}

			return true;
		}
		
		private bool IsReadyForPopUp(COfferGroup group)
		{
			EOfferPlacement placement = group.GetParamValueOrDefault<EOfferPlacement>(EOfferParam.OfferPlacement);
			if(!placement.HasFlag(EOfferPlacement.Popup))
				return false;

			if(!IsTimeToPopUp(group.GroupId, group.Params))
				return false;

			COffer[] offersInGroup = _user.Offers.GetOffersByParam(EOfferParam.GroupId, group.GroupId);
			if(offersInGroup.All(offer => offer.IsExpired(_serverTime.GetTimestampInMs())))
				return false;
			
			if(!_user.Offers.IsGroupValid(group.GroupId))
				return false;
			
			return true;
		}
		
		private bool IsTimeToPopUp(string guid, IOfferParam[] offerParams)
		{
			long lastPopUpTime = GetLastPopUpTime(guid);
			if (offerParams.All(param => param.Id != EOfferParam.PopupIntervalInSeconds))
			{
				return lastPopUpTime == -1;
			}
			
			if(lastPopUpTime == -1)
				return true;

			long popUpIntervalInMs = offerParams.First(param => param.Id == EOfferParam.PopupIntervalInSeconds).GetValue<long>() * CTimeConst.Second.InMilliseconds;
			return _serverTime.GetTimestampInMs() > lastPopUpTime + popUpIntervalInMs;
		}
		
		private long GetLastPopUpTime(string guid)
		{
			return _lastPopUpTimes.GetValueOrDefault(guid, -1);
		}
		
		private void UpdateLastPopUpTime(string guid)
		{
			long currentTime = _serverTime.GetTimestampInMs();
			_lastPopUpTimes[guid] = currentTime;
			SaveLastPopUpTimes();
		}
		
		private void ClearOldPopUpTimes()
		{
			HashSet<string> validGuids = new HashSet<string>(_user.Offers.GetOffersWithParam(EOfferParam.OfferPlacement).Select(o => o.Guid));
			
			string[] existingEntries = _lastPopUpTimes.Keys.ToArray();
			foreach (string key in existingEntries)
			{
				if (!validGuids.Contains(key))
					_lastPopUpTimes.Remove(key);
			}
		}

		private void SaveLastPopUpTimes()
		{
			string serialized = string.Join(';', _lastPopUpTimes.Select(item => $"{item.Key};{item.Value}"));
			CPlayerPrefs.Set("LastOfferPopUpTimes", serialized);
		}
		
		private void LoadLastPopUpTimes()
		{
			_lastPopUpTimes.Clear();
			string serialized = CPlayerPrefs.Get("LastOfferPopUpTimes", string.Empty);
			if (string.IsNullOrEmpty(serialized))
				return;

			string[] entries = serialized.Split(';');
			for (int i = 0; i < entries.Length - 1; i += 2)
			{
				string guid = entries[i];
				long timestamp = long.Parse(entries[i + 1]);
				_lastPopUpTimes[guid] = timestamp;
			}
		}

		private class CPopUpOfferAction : ILazyAction
		{
			public int Priority => 1;
			private readonly Func<CancellationToken, UniTask> _popUpAction;

			public CPopUpOfferAction(Func<CancellationToken, UniTask> popUpAction)
			{
				_popUpAction = popUpAction;
			}
			
			public async UniTask Execute(CancellationToken ct)
			{
				await _popUpAction(ct);
			}
		}
	}
}