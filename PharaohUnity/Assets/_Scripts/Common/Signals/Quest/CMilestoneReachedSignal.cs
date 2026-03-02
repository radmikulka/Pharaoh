using AldaEngine;
using ServerData;

namespace Pharaoh
{
	public class CMilestoneReachedSignal : IEventBusSignal
	{
		public readonly int ChapterIndex;
		public readonly int MilestoneIndex;
		public readonly SResource[] Reward;

		public CMilestoneReachedSignal(int chapterIndex, int milestoneIndex, SResource[] reward)
		{
			ChapterIndex = chapterIndex;
			MilestoneIndex = milestoneIndex;
			Reward = reward;
		}
	}
}
