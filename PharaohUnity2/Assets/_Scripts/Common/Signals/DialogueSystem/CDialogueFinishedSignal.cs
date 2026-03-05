// =========================================
// AUTHOR: Marek Karaba
// DATE:   04.07.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CDialogueFinishedSignal : IEventBusSignal
	{
		public EDialogueId DialogueId { get; }
		public int CurrentDialogueCommandsCount { get; }
		public int DurationInMs { get; }

		public CDialogueFinishedSignal(EDialogueId dialogueId, int durationInMs, int currentDialogueCommandsCount)
		{
			DialogueId = dialogueId;
			DurationInMs = durationInMs;
			CurrentDialogueCommandsCount = currentDialogueCommandsCount;
		}
	}
}