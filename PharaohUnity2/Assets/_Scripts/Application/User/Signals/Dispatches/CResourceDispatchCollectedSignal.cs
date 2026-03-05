// =========================================
// AUTHOR: Marek Karaba
// DATE:   23.09.2025
// =========================================

using AldaEngine;

namespace TycoonBuilder
{
	public class CResourceDispatchCollectedSignal : IEventBusSignal
	{
		public readonly CDispatch Dispatch;

		public CResourceDispatchCollectedSignal(CDispatch dispatch)
		{
			Dispatch = dispatch;
		}
	}
}