// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.03.2026
// =========================================

using AldaEngine;

namespace TycoonBuilder
{
	public class CDispatchCancelledSignal : IEventBusSignal
	{
		public readonly CDispatch Dispatch;

		public CDispatchCancelledSignal(CDispatch dispatch)
		{
			Dispatch = dispatch;
		}
	}
}