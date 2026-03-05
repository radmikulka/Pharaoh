// =========================================
// AUTHOR: Marek Karaba
// DATE:   04.07.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CShowLiveEventMenuDialogSignal : IEventBusSignal
	{
		public readonly EDialogueId DialogueId;
		
		public CShowLiveEventMenuDialogSignal(EDialogueId id)
		{
			DialogueId = id;
		}
	}
}