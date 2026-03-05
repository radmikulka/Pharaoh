// =========================================
// AUTHOR: Radek Mikulka
// DATE:   31.07.2025
// =========================================

using AldaEngine;
using ServerData;
using ServerData.Dto;

namespace TycoonBuilder
{
	public class CVehicleDispatchedSignal : IEventBusSignal
	{
		public readonly CDispatch Dispatch;

		public CVehicleDispatchedSignal(CDispatch dispatch)
		{
			Dispatch = dispatch;
		}
	}
}