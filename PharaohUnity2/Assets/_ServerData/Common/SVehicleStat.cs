// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.08.2025
// =========================================

using ServerData;

namespace ServerData
{
	public struct SVehicleStat
	{
		public EVehicleStat Stat;
		public int Level;

		public SVehicleStat(EVehicleStat stat, int level)
		{
			Stat = stat;
			Level = level;
		}
	}
}