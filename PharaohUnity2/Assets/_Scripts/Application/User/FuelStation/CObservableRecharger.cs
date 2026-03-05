// =========================================
// AUTHOR: Radek Mikulka
// DATE:   11.09.2025
// =========================================

using System;
using ServerData;

namespace TycoonBuilder
{
	public class CObservableRecharger
	{
		private CRecharger _recharger;
		public event Action<SValueChangeArgs> ValueChanged; 
		public int CurrentAmount => _recharger?.CurrentAmount ?? 0;
		private int _lastSeenMaxCapacity;
		public long LastTickTime => _recharger.LastTickTime;
		public int ProductionPerTick => _recharger.ProductionPerTick;
		public long ProductionTickDurationInSecs => _recharger.ProductionTickDurationInSecs;
		
		public CObservableRecharger()
		{
			SetRecharger(CRecharger.Empty(), 0);
		}

		public int MaxCapacity(long time)
		{
			return _recharger?.MaxCapacity(time) ?? 0;
		}

		public void SetRecharger(CRecharger recharger, long time)
		{
			CallWithValueChangeCheck(() => _recharger = recharger, time);
		}

		public void Upgrade(SRechargerLevelConfig rechargerConfig, long time)
		{
			CallWithValueChangeCheck(() => _recharger.Upgrade(rechargerConfig, time), time);
		}

		public void ModifyOverCapacity(int amount, long time, CValueModifyParams modifyParams)
		{
			CallWithValueChangeCheck(() => _recharger.ModifyOverCapacity(amount), time, modifyParams);
		}

		public void Remove(int amount, long timestampInMs)
		{
			CallWithValueChangeCheck(() => _recharger.Remove(amount, timestampInMs), timestampInMs);
		}

		public void Update(long timestamp)
		{
			// hard coded content of CallWithValueChangeCheck for GC optimization reason
			int prevValue = CurrentAmount;
			_recharger.Update(timestamp);
			if (prevValue != CurrentAmount || _lastSeenMaxCapacity != MaxCapacity(timestamp))
			{
				ValueChanged?.Invoke(new SValueChangeArgs(null, prevValue, CurrentAmount));
				_lastSeenMaxCapacity = MaxCapacity(timestamp);
			}
		}

		private void CallWithValueChangeCheck(Action action, long time, CValueModifyParams modifyParams = null)
		{
			int prevValue = CurrentAmount;
			action();
			if (prevValue != CurrentAmount || _lastSeenMaxCapacity != MaxCapacity(time))
			{
				ValueChanged?.Invoke(new SValueChangeArgs(modifyParams, prevValue, CurrentAmount));
				_lastSeenMaxCapacity = MaxCapacity(time);
			}
		}

		public bool CompareWithDto(CRechargerDto dto)
		{
			return _recharger.CompareWithDto(dto);
		}

		public override string ToString()
		{
			return _recharger.ToString();
		}

		public long GetNextRechargeRemainingTime(long timestampInMs)
		{
			return _recharger.GetNextRechargeRemainingTime(timestampInMs);
		}

		public long GetFullRechargeRemainingTime(long timestampInMs)
		{
			return _recharger.GetFullRechargeRemainingTime(timestampInMs);
		}
	}
}