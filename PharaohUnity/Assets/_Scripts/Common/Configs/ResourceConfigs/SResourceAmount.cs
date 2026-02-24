using System;
using RoboRyanTron.SearchableEnum;
using ServerData;

namespace Pharaoh
{
	[Serializable]
	public struct SResourceAmount
	{
		[SearchableEnum] public EResource Resource;
		public int Amount;
	}
}
