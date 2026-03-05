// =========================================
// AUTHOR: Marek Karaba
// DATE:   14.01.2026
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using KBCore.Refs;
using ServerData;
using TycoonBuilder.Ui;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CUiTopBarItemEventIcon : CUiTopBarItemIcon, IInitializable
	{
		[SerializeField, Self] private CUiComponentImage _iconImage;
		
		private CResourceConfigs _resourceConfigs;
		private IBundleManager _bundleManager;
		private IEventBus _eventBus;
		private CUser _user;

		private ELiveEvent _activeEventId;
		
		[Inject]
		private void Inject(
			CResourceConfigs resourceConfigs,
			IBundleManager bundleManager,
			IEventBus eventBus,
			CUser user
			)
		{
			_resourceConfigs = resourceConfigs;
			_bundleManager = bundleManager;
			_eventBus = eventBus;
			_user = user;
		}

		public void Initialize()
		{
			_eventBus.Subscribe<CLiveEventsSyncedSignal>(OnLiveEventSynced);
			_eventBus.Subscribe<CLiveEventCurrencyTypeChangedSignal>(OnCurrencyTypeChanged);
		}

		internal override void SetIcon(ETopBarItem id)
		{
			if (id != ETopBarItem.EventCoin)
			{
				Debug.LogError($"CUiTopBarItemEventIcon: Unsupported valuable id {id}!");
				return;
			}

			SetIconVisual();
		}

		private void OnLiveEventSynced(CLiveEventsSyncedSignal signal)
		{
			SetIconVisual();
		}
		
		private void OnCurrencyTypeChanged(CLiveEventCurrencyTypeChangedSignal signal)
		{
			_activeEventId = signal.LiveEventId;
			SetIconVisual();
		}

		private void SetIconVisual()
		{
			if (_activeEventId == ELiveEvent.None)
			{
				_activeEventId = _user.LiveEvents.GetFirstActiveEventId();
			}
			
			if (_activeEventId == ELiveEvent.None)
				return;

			CLiveEventResourceConfig config = _resourceConfigs.LiveEvents.GetConfig(_activeEventId);
			Sprite icon = _bundleManager.LoadItem<Sprite>(config.EventCoinsIconSprite, EBundleCacheType.Persistent);
			_iconImage.SetSprite(icon);
		}
	}
}