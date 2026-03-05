// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.09.2025
// =========================================

namespace ServerData
{
	public class CCityStat
	{
		public readonly ECityStat Stat;
		public readonly int Value;
		
		public CCityStat(ECityStat stat, int value)
		{
			Stat = stat;
			Value = value;
		}
	}
}