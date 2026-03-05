// =========================================
// AUTHOR: Radek Mikulka
// DATE:   10.07.2025
// =========================================

using System.Collections.Generic;

namespace ServerData
{
	public class CContractTask
	{
		public readonly IContractTaskComponent Component;
		private readonly IValuable[] _rewards;

		public readonly bool AllowGoTo;
		public readonly bool CloseDispatchMenuWhenIsFullyLoaded;
		public readonly bool CloseDispatchMenuWhenIsFullyLoadedAndFollowVehicle;
		public bool IsFleetTask => Component is CTransportFleetTaskConfig;
		public IReadOnlyList<IValuable> Rewards => _rewards;

		public CContractTask(
			IContractTaskComponent component,
			IValuable[] rewards,
			bool closeDispatchMenuWhenIsFullyLoaded,
			bool closeDispatchMenuWhenIsFullyLoadedAndFollowVehicle,
			bool allowGoTo
			)
		{
			CloseDispatchMenuWhenIsFullyLoadedAndFollowVehicle = closeDispatchMenuWhenIsFullyLoadedAndFollowVehicle;
			CloseDispatchMenuWhenIsFullyLoaded = closeDispatchMenuWhenIsFullyLoaded;
			AllowGoTo = allowGoTo;
			Component = component;
			_rewards = rewards;
		}
	}
}
