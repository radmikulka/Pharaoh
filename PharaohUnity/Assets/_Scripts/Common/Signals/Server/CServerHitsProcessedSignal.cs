// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

using System.Linq;
using AldaEngine;
using AldaEngine.Tcp;
using ServerData.Hits;

namespace Pharaoh
{
	public class CServerHitsProcessedSignal : IEventBusSignal
	{
		public readonly IHit[] Hits;
		
		public CServerHitsProcessedSignal(IHit[] hits)
		{
			Hits = hits;
		}

		public T GetHitOrDefault<T>() where T : CResponseHit
		{
			return Hits.FirstOrDefault(hit => hit is T) as T;
		}
	}
}