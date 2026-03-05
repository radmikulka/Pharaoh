// =========================================
// AUTHOR: Radek Mikulka
// DATE:   03.09.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CContractActivatedSignal : IEventBusSignal
	{
		public readonly CContract StoryContract;

		public CContractActivatedSignal(CContract storyContract)
		{
			StoryContract = storyContract;
		}
	}
}