// =========================================
// AUTHOR: Radek Mikulka
// DATE:   07.10.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CResourceDispatchCompletedSignal : IEventBusSignal
	{
		public readonly SResource Resource;
		public readonly string Uid;

		public CResourceDispatchCompletedSignal(string uid, SResource resource)
		{
			Resource = resource;
			Uid = uid;
		}
	}
}