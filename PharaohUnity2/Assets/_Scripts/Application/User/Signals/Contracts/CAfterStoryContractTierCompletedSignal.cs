// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.02.2026
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CAfterStoryContractTierCompletedSignal : IEventBusSignal
	{
		public readonly CContract StoryContract;

		public CAfterStoryContractTierCompletedSignal(CContract storyContract)
		{
			StoryContract = storyContract;
		}
	}
}