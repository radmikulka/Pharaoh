// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.09.2025
// =========================================

using System;
using AldaEngine;

namespace ServerData
{
	public class CRecharger : IMapAble
	{
		private IRechargerConfig _config;

		public SUnixTime LastTickTime { get; private set; }
		public int CurrentAmount { get; private set; }
		public int ProductionPerTick => _config.ProductionPerTick;
		public long ProductionTickDurationInMs => _config.ProductionTickDurationInMs;
		public long ProductionTickDurationInSecs => _config.ProductionTickDurationInMs / CTimeConst.Second.InMilliseconds;
		public event Action OnRecharged;

		public static CRecharger New(IRechargerConfig config)
		{
			return new CRecharger(0, config.MaxCapacity, config);
		}
		
		public static CRecharger Empty()
		{
			return new CRecharger(0, 0, new SRechargerLevelConfig());
		}

		public static CRecharger Existing(
			long lastTickTime,
			int currentAmount,
			IRechargerConfig config
		)
		{
			return new CRecharger(lastTickTime, currentAmount, config);
		}

		protected CRecharger(
			long lastTickTime, 
			int currentAmount, 
			IRechargerConfig config
		)
		{
			CurrentAmount = currentAmount;
			LastTickTime = lastTickTime;
			_config = config;
		}
		
		
		public virtual int MaxCapacity(long time) => _config.MaxCapacity;

		public void Upgrade(IRechargerConfig newConfig, long time)
		{
			Update(time);
			
			bool isFull = CurrentAmount >= MaxCapacity(time);
			if (isFull)
			{
				LastTickTime = time;
			}

			_config = newConfig;
		}

		public void Refill(long time)
		{
			CurrentAmount = MaxCapacity(time);
		}
		
		public void Add(int amount, long time)
		{
			if(CurrentAmount >= MaxCapacity(time))
				return;
			
			CurrentAmount += amount;
			CurrentAmount = CMath.Min(CurrentAmount, MaxCapacity(time));
		}
		
		public void ModifyOverCapacity(int amount)
		{
			CurrentAmount += amount;
			CurrentAmount = CMath.Max(0, CurrentAmount);
		}

		public void Remove(int amount, long time)
		{
			Update(time);
			
			if (CurrentAmount == MaxCapacity(time))
			{
				LastTickTime = time;
			}
			CurrentAmount -= amount;
			CurrentAmount = CMath.Max(CurrentAmount, 0);
		}

		public void Update(long time)
		{
			while (true)
			{
				if (CurrentAmount >= MaxCapacity(time))
					break;
				
				if(LastTickTime == 0)
				{
					LastTickTime = time;
				}
			
				long nextGainTime = LastTickTime + _config.ProductionTickDurationInMs;
				if(nextGainTime > time)
					break;
				
				CurrentAmount = CMath.Min(CurrentAmount + _config.ProductionPerTick, MaxCapacity(time));
			
				LastTickTime = CurrentAmount < MaxCapacity(time) ? nextGainTime : 0;
				OnRecharged?.Invoke();
			}
		}

		public void HaveCurrentAmountOrThrow(int amount, long timestampInMs)
		{
			Update(timestampInMs);
			
			if (CurrentAmount < amount)
			{
				throw new InvalidOperationException("Not enough amount in recharger.");
			}
		}

		public long GetNextRechargeRemainingTime(long timestampInMs)
		{
			if (CurrentAmount >= MaxCapacity(timestampInMs))
				return 0;
			
			Update(timestampInMs);
			long nextIncreaseTime = LastTickTime + _config.ProductionTickDurationInMs;
			return CMath.Max(0, nextIncreaseTime - timestampInMs);
		}

		public long GetFullRechargeRemainingTime(long timestampInMs)
		{
			if (CurrentAmount >= MaxCapacity(timestampInMs))
				return 0;
			
			Update(timestampInMs);
			int missingAmount = MaxCapacity(timestampInMs) - CurrentAmount;
			int neededTicks = CMath.CeilToInt( (float) missingAmount / _config.ProductionPerTick);
			long nextIncreaseTime = LastTickTime + _config.ProductionTickDurationInMs;
			return CMath.Max(0, nextIncreaseTime - timestampInMs) + (neededTicks - 1) * _config.ProductionTickDurationInMs;
		}

		public long GetTimeRemainingForNextCharge(long timestampInMs)
		{
			if (CurrentAmount >= MaxCapacity(timestampInMs))
				return 0;
			
			Update(timestampInMs);
			long nextIncreaseTime = LastTickTime + _config.ProductionTickDurationInMs;
			return CMath.Max(0, nextIncreaseTime - timestampInMs);
		}

		public long GetTimeRemainingForFullCharge(long timestampInMs)
		{
			if (CurrentAmount >= MaxCapacity(timestampInMs))
				return 0;
			
			Update(timestampInMs);
			int missingAmount = MaxCapacity(timestampInMs) - CurrentAmount;
			int neededTicks = CMath.CeilToInt( (float) missingAmount / _config.ProductionPerTick);
			long nextIncreaseTime = LastTickTime + _config.ProductionTickDurationInMs;
			return CMath.Max(0, nextIncreaseTime - timestampInMs) + (neededTicks - 1) * _config.ProductionTickDurationInMs;
		}
		
		public bool CompareWithDto(CRechargerDto dto)
		{
			return dto.CurrentAmount == CurrentAmount &&
			       dto.LastTickTime == LastTickTime;
		}

		public override string ToString()
		{
			return $"{nameof(LastTickTime)}: {LastTickTime} ({CUnixTime.GetDate(LastTickTime)}), {nameof(CurrentAmount)}: {CurrentAmount}";
		}

		public bool IsFull(long time)
		{
			return CurrentAmount >= MaxCapacity(time);
		}
	}
}