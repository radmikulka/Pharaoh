// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.02.2026
// =========================================

using System.Linq;

namespace ServerData
{
	public class CSpecialBonusCollection
	{
		private readonly ISpecialBonus[] _bonuses;
		
		public readonly ESpecialBonusRewardSource Source;

		public CSpecialBonusCollection(ESpecialBonusRewardSource source, ISpecialBonus[] bonuses)
		{
			_bonuses = bonuses;
			Source = source;
		}

		public int GetBonus(ESpecialBonusRewardType type)
		{
			return ((CIntSpecialBonus)_bonuses.First(bonus => bonus.Type == type)).Value;
		}
	}
}