// =========================================
// AUTHOR: Juraj Joscak
// DATE:   02.09.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CStoryContractInstanceUpgradeCompleteSignal : IEventBusSignal
	{
		public readonly CContract StoryContract;
		
		public CStoryContractInstanceUpgradeCompleteSignal(CContract storyContract)
		{
			StoryContract = storyContract;
		}
	}
}