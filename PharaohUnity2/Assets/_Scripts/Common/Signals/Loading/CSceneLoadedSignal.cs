// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.09.2025
// =========================================

using AldaEngine;
using ServerData;

namespace Pharaoh
{
	public class CSceneLoadedSignal : IEventBusSignal
	{
		public readonly ESceneId Id;

		public CSceneLoadedSignal(ESceneId id)
		{
			Id = id;
		}
	}
}