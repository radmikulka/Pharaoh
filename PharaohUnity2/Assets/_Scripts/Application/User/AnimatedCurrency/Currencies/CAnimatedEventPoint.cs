// =========================================
// AUTHOR: Marek Karaba
// DATE:   19.02.2026
// =========================================

using System;
using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CAnimatedEventPoint : IAnimatedCurrency
	{
		public event Action<SValueChangeArgs> ValueChanged;
		public int Value => GetValue();
		private readonly CUser _user;
		
		public CAnimatedEventPoint(CUser user, IEventBus eventBus)
		{
			eventBus.Subscribe<COwnedValuableChangedSignal>(OnOwnedValuableChanged);
			eventBus.Subscribe<CLiveEventsSyncedSignal>(OnLiveEventSynced);
			_user = user;
		}

		private int GetValue()
		{
			ELiveEvent activeEventId = _user.LiveEvents.GetFirstActiveEventId();
			if (activeEventId == ELiveEvent.None)
				return 0;

			if (_user.LiveEvents.GetActiveEvent(activeEventId)?.BaseContent is not IEventWithStore liveEvent)
				return 0;
			
			return liveEvent.EventCoins;
		}
		
		private void OnOwnedValuableChanged(COwnedValuableChangedSignal signal)
		{
			if (signal.Valuable.Id != EValuable.EventPoint)
				return;
			
			CEventPointValuable eventPoint = (CEventPointValuable)signal.Valuable;
			CValueModifyParams changeParams = new CValueModifyParams().Add(new CAnimatedChangeParam());
			ValueChanged?.Invoke(new SValueChangeArgs(changeParams, Value-eventPoint.Amount, Value));
		}
		
		private void OnLiveEventSynced(CLiveEventsSyncedSignal signal)
		{
			ValueChanged?.Invoke(new SValueChangeArgs(new CValueModifyParams(), Value, Value));
		}
		
		public void Tick(long timestamp) { }
	}
}