using System;

namespace Pharaoh
{
	[Serializable]
	public struct SBuildingLevelData
	{
		public SResourceAmount[] Upkeep;
		public SResourceAmount[] Production;
	}
}
