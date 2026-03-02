using AldaEngine;

namespace Pharaoh
{
	public class CQuestClaimedSignal : IEventBusSignal
	{
		public readonly int SlotIndex;

		public CQuestClaimedSignal(int slotIndex)
		{
			SlotIndex = slotIndex;
		}
	}
}
