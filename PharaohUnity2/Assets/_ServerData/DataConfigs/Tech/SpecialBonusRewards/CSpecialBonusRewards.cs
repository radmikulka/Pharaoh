// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.02.2026
// =========================================

using System;
using System.Collections.Generic;

namespace ServerData
{
	public class CSpecialBonusRewards
	{
		private readonly Dictionary<ESpecialBonusRewardSource, Dictionary<ERegion, CSpecialBonusCollection>> _decadePassRewards = new();

		public CSpecialBonusCollection GetBonuses(ESpecialBonusRewardSource source, ERegion region)
		{
			return _decadePassRewards[source][region];
		}
		
		protected void SetBonusRewards(ESpecialBonusRewardSource source, ERegion region, params ISpecialBonus[] rewards)
		{
			if (!_decadePassRewards.TryGetValue(source, out var collection))
			{
				collection = new Dictionary<ERegion, CSpecialBonusCollection>();
				_decadePassRewards.Add(source, collection);
			}
			
			collection[region] = new CSpecialBonusCollection(source, rewards);
		}
	}
}