// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

namespace ServerData
{
	public class CRechargerConfig
	{
		private readonly CRechargerLevel[] _levels;

		public CRechargerConfig(CRechargerLevel[] levels)
		{
			_levels = levels;
		}
	}
}