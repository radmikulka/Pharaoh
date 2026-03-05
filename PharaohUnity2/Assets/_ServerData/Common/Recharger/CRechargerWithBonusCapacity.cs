// =========================================
// AUTHOR: Juraj Joscak
// DATE:   21.01.2026
// =========================================

using System;

namespace ServerData
{
	public class CRechargerWithBonusCapacity : CRecharger
	{
		public delegate int BonusCapacityProvider(long time);
		
		private BonusCapacityProvider _bonusCapacityProvider;
		
		public static CRechargerWithBonusCapacity New(IRechargerConfig config, BonusCapacityProvider bonusCapacityProvider)
		{
			return new CRechargerWithBonusCapacity(0, config.MaxCapacity, config, bonusCapacityProvider);
		}
		
		public static CRechargerWithBonusCapacity Existing(
			long lastTickTime,
			int currentAmount,
			IRechargerConfig config,
			BonusCapacityProvider bonusCapacityProvider
			)
		{
			return new CRechargerWithBonusCapacity(lastTickTime, currentAmount, config, bonusCapacityProvider);
		}

		private CRechargerWithBonusCapacity(long lastTickTime, int currentAmount, IRechargerConfig config, BonusCapacityProvider bonusCapacityProvider)
			: base(lastTickTime, currentAmount, config)
		{
			_bonusCapacityProvider = bonusCapacityProvider;
		}

		public override int MaxCapacity(long time)
		{
			return base.MaxCapacity(time) + GetBonusCapacity(time);
		}

		private int GetBonusCapacity(long time)
		{
			return _bonusCapacityProvider?.Invoke(time) ?? 0;
		}
		
		public void SetBonusCapacityProvider(BonusCapacityProvider bonusCapacityProvider)
		{
			_bonusCapacityProvider = bonusCapacityProvider;
		}
	}
}