using AldaEngine;

namespace Pharaoh
{
	public class CChapterCompletedSignal : IEventBusSignal
	{
		public readonly int CompletedChapterIndex;

		public CChapterCompletedSignal(int completedChapterIndex)
		{
			CompletedChapterIndex = completedChapterIndex;
		}
	}
}
