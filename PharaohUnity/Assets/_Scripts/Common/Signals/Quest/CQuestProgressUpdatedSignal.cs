using AldaEngine;

namespace Pharaoh
{
	public class CQuestProgressUpdatedSignal : IEventBusSignal
	{
		public readonly int SlotIndex;
		public readonly float Progress;
		public readonly float Target;

		public CQuestProgressUpdatedSignal(int slotIndex, float progress, float target)
		{
			SlotIndex = slotIndex;
			Progress = progress;
			Target = target;
		}
	}
}
