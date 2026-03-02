using System;
using ServerData;

namespace Pharaoh
{
	[Serializable]
	public struct SBuildingLevelData
	{
		public SResource[] Upkeep;
		public SResource[] Production;
		public SResource[] LevelCost;
		public IUnlockRequirement LevelUpRequirement;
	}
}
