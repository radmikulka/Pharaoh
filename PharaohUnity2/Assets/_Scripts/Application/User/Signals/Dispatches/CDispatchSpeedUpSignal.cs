// =========================================
// AUTHOR: Radek Mikulka
// DATE:   10.09.2025
// =========================================

using AldaEngine;

namespace TycoonBuilder
{
	public class CDispatchSpeedUpSignal : IEventBusSignal
	{
		public readonly CDispatch Dispatch;

		public CDispatchSpeedUpSignal(CDispatch dispatch)
		{
			Dispatch = dispatch;
		}
	}
}