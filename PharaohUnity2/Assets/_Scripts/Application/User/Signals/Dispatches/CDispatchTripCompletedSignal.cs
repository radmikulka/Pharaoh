// =========================================
// AUTHOR: Marek Karaba
// DATE:   02.03.2026
// =========================================

using AldaEngine;

namespace TycoonBuilder
{
	public class CDispatchTripCompletedSignal : IEventBusSignal
	{
		public readonly CDispatch Dispatch;

		public CDispatchTripCompletedSignal(CDispatch dispatch)
		{
			Dispatch = dispatch;
		}
	}
}

