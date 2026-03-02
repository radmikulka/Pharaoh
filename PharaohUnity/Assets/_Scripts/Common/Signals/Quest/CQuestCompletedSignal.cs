using AldaEngine;

namespace Pharaoh
{
	public class CQuestCompletedSignal : IEventBusSignal
	{
		public readonly int SlotIndex;

		public CQuestCompletedSignal(int slotIndex)
		{
			SlotIndex = slotIndex;
		}
	}
}
