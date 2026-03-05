// =========================================
// AUTHOR: Marek Karaba
// DATE:   10.12.2025
// =========================================

using System;
using AldaEngine;
using AldaEngine.AldaFramework;
using KBCore.Refs;
using ServerData;
using TycoonBuilder.Offers;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CActiveLiveEventVisual : ValidatedMonoBehaviour, IInitializable
	{
		[SerializeField] private GameObject _visual;
		[SerializeField] private CUiButton _button;
		[SerializeField, Child] private CUiCountdown _timer;
		[SerializeField] private CUiComponentImage _bannerImage;
		[SerializeField] private CUiComponentImage _iconImage;
		[SerializeField] private CUiComponentText _title;
		[SerializeField] private CEventHudButtonMarker _marker;
		
		private CResourceConfigs _resourceConfigs;
		private IBundleManager _bundleManager;
		private ICtsProvider _ctsProvider;
		private IServerTime _serverTime;
		private IEventBus _eventBus;
		private CUser _user;
		private ITranslation _translation;
		
		private bool IsActive => _visual.activeSelf;
		private ILiveEvent _liveEvent;
		
		[Inject]
		private void Inject(
			CResourceConfigs resourceConfigs,
			IBundleManager bundleManager,
			ICtsProvider ctsProvider,
			IServerTime serverTime,
			IEventBus eventBus,
			CUser user,
			ITranslation translation)
		{
			_resourceConfigs = resourceConfigs;
			_bundleManager = bundleManager;
			_ctsProvider = ctsProvider;
			_serverTime = serverTime;
			_eventBus = eventBus;
			_user = user;
			_translation = translation;
		}
		
		public void Initialize()
		{
			_button.AddClickListener(LoadEvent);
			_eventBus.Subscribe<CLeaderboardRewardClaimedSignal>(OnLeaderboardRewardClaimed);
			_eventBus.Subscribe<CEventPassRewardClaimedSignal>(OnEventPassRewardClaimed);
			_eventBus.Subscribe<CLiveEventFinishedSignal>(OnLiveEventFinished);
			_eventBus.Subscribe<COwnedValuableChangedSignal>(OnOwnedValuableChanged);
			_eventBus.Subscribe<CEventPassActivatedSignal>(OnEventPassActivated);
		}

		private void OnLeaderboardRewardClaimed(CLeaderboardRewardClaimedSignal signal)
		{
			UpdateMarker(signal.LiveEventId);
		}

		private void OnEventPassRewardClaimed(CEventPassRewardClaimedSignal signal)
		{
			UpdateMarker(signal.LiveEventId);
		}

		private void OnLiveEventFinished(CLiveEventFinishedSignal signal)
		{
			UpdateMarker(signal.LiveEventId);
		}

		private void OnOwnedValuableChanged(COwnedValuableChangedSignal signal)
		{
			if (signal.Valuable is not CEventPointValuable eventPointValuable) 
				return;

			if (_liveEvent?.Id != eventPointValuable.LiveEvent)
				return;
			
			UpdateMarker(eventPointValuable.LiveEvent);
		}

		private void OnEventPassActivated(CEventPassActivatedSignal signal)
		{
			UpdateMarker(signal.LiveEventId);
		}

		private void UpdateMarker(ELiveEvent liveEventId)
		{
			if (_liveEvent?.Id != liveEventId)
				return;

			_marker.SetMarkerState(liveEventId);
		}

		private void Update()
		{
			if (!IsActive || _liveEvent == null)
				return;

			SetTimer();
		}

		private void SetTimer()
		{
			long timeRemaining = _liveEvent.EndTimeInMs - _serverTime.GetTimestampInMs();
			if (timeRemaining > 0)
			{
				_timer.SetTime(timeRemaining, IAldaTimeSpanProvider.ProviderOnlyNecessaryPartsWithSeconds);
				return;
			}
			_liveEvent = null;
		}

		public void Show(ILiveEvent liveEvent)
		{
			_visual.SetActive(true);
			_liveEvent = liveEvent;

			SetIcons();
			_title.SetValue(_translation.GetText($"LiveEvent.{(int)_liveEvent.Id}.Title"));
			_marker.SetMarkerState(liveEvent.Id);
		}

		private void SetIcons()
		{
			CLiveEventResourceConfig config = _resourceConfigs.LiveEvents.GetConfig(_liveEvent.Id);
			Sprite bannerSprite = _bundleManager.LoadItem<Sprite>(config.ActiveHudBannerIconSprite);
			Sprite iconSprite = _bundleManager.LoadItem<Sprite>(config.MainIconSprite);
			_bannerImage.SetSprite(bannerSprite);
			_iconImage.SetSprite(iconSprite);
		}

		public void Hide()
		{
			_visual.SetActive(false);
		}

		private void LoadEvent()
		{
			if (!IsActive)
				return;
			
			IEventWithLeaderboard content = _user.LiveEvents.GetEventContent<IEventWithLeaderboard>(_liveEvent.Id);
			bool? isLeaderboardFinished = content?.Leaderboard?.IsFinished(_serverTime.GetTimestampInMs());
			EEventOverviewTab tab = isLeaderboardFinished is true
				? EEventOverviewTab.Leaderboard
				: EEventOverviewTab.None;
			_eventBus.ProcessTaskAsync(new COpenEventOverviewTask(_liveEvent.Id, tab), _ctsProvider.Token);
		}
	}
}