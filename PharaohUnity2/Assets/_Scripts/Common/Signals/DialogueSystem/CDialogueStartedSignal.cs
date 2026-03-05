// =========================================
// AUTHOR: Marek Karaba
// DATE:   04.07.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CDialogueStartedSignal : IEventBusSignal
	{
		public EDialogueId DialogueId { get; }
		public int CurrentDialogueCommandsCount { get; }

		public CDialogueStartedSignal(EDialogueId dialogueId, int currentDialogueCommandsCount)
		{
			DialogueId = dialogueId;
			CurrentDialogueCommandsCount = currentDialogueCommandsCount;
		}
	}
}