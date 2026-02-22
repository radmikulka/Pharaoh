// =========================================
// DATE:   22.02.2026
// =========================================

using AldaEngine;
using ServerData;

namespace Pharaoh
{
	public class COwnedResourceChangedSignal : IEventBusSignal
	{
		public readonly SResource Resource;
		public readonly EMissionId Mission;
		public readonly SValueChangeArgs ChangeArgs;

		public COwnedResourceChangedSignal(SResource resource, EMissionId mission, SValueChangeArgs changeArgs)
		{
			Resource = resource;
			Mission = mission;
			ChangeArgs = changeArgs;
		}
	}
}
