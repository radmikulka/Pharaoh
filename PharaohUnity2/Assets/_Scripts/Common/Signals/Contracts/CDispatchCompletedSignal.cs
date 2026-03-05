// =========================================
// AUTHOR: Radek Mikulka
// DATE:   22.09.2025
// =========================================

using AldaEngine;

namespace TycoonBuilder
{
	public class CDispatchCompletedSignal : IEventBusSignal
	{
		public readonly string Uid;

		public CDispatchCompletedSignal(string uid)
		{
			Uid = uid;
		}
	}
}