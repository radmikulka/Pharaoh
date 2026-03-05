// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.02.2026
// =========================================

namespace ServerData
{
	public class CIntSpecialBonus : ISpecialBonus
	{
		public readonly int Value;
		public ESpecialBonusRewardType Type { get; }

		public CIntSpecialBonus(ESpecialBonusRewardType type, int value)
		{
			Type = type;
			Value = value;
		}
	}
}