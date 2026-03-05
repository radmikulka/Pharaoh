// =========================================
// AUTHOR: Juraj Joscak
// DATE:   06.10.2025
// =========================================

using AldaEngine;

namespace TycoonBuilder
{
	public class CPulseMissingRequirementSignal : IEventBusSignal
	{
		public readonly EPulsatingRequirementPlacement Placement;
		
		public CPulseMissingRequirementSignal(EPulsatingRequirementPlacement placement)
		{
			Placement = placement;
		}
	}
}