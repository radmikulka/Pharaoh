// =========================================
// AUTHOR: Juraj Joscak
// DATE:   06.01.2026
// =========================================

using System;
using AldaEngine;
using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	public class CAnimatedEventCoin : IAnimatedCurrency
	{
		public event Action<SValueChangeArgs> ValueChanged;
		public int Value => GetValue();
		private readonly CUser _user;
		private ELiveEvent _activeEventId;
		
		public CAnimatedEventCoin(CUser user, IEventBus eventBus)
		{
			eventBus.Subscribe<COwnedValuableChangedSignal>(OnOwnedValuableChanged);
			eventBus.Subscribe<CLiveEventsSyncedSignal>(OnLiveEventSynced);
			eventBus.Subscribe<CLiveEventCurrencyTypeChangedSignal>(OnLiveEventCurrencyTypeChanged);
			_user = user;
		}

		private int GetValue()
		{
			if (_activeEventId == ELiveEvent.None)
			{
				_activeEventId = _user.LiveEvents.GetFirstActiveEventId();
			}
			
			if (_activeEventId == ELiveEvent.None)
				return 0;

			if (_user.LiveEvents.GetActiveEvent(_activeEventId)?.BaseContent is not IEventWithStore liveEvent)
				return 0;
			
			return liveEvent.EventCoins;
		}
		
		private void OnLiveEventCurrencyTypeChanged(CLiveEventCurrencyTypeChangedSignal signal)
		{
			_activeEventId = signal.LiveEventId;
			ValueChanged?.Invoke(new SValueChangeArgs(new CValueModifyParams(), Value, Value));
		}
		
		private void OnOwnedValuableChanged(COwnedValuableChangedSignal signal)
		{
			if (signal.Valuable.Id != EValuable.EventCoin)
				return;
			
			CEventCoinValuable eventCoin = (CEventCoinValuable)signal.Valuable;
			CValueModifyParams changeParams = new CValueModifyParams().Add(new CAnimatedChangeParam());
			ValueChanged?.Invoke(new SValueChangeArgs(changeParams, Value-eventCoin.Amount, Value));
		}
		
		private void OnLiveEventSynced(CLiveEventsSyncedSignal signal)
		{
			ValueChanged?.Invoke(new SValueChangeArgs(new CValueModifyParams(), Value, Value));
		}
		
		public void Tick(long timestamp) { }
	}
}