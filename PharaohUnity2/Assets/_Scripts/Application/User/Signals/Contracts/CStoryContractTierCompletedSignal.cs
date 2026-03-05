// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.08.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CStoryContractTierCompletedSignal : IEventBusSignal
	{
		public readonly CContract StoryContract;

		public CStoryContractTierCompletedSignal(CContract storyContract)
		{
			StoryContract = storyContract;
		}
	}
}