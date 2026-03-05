// =========================================
// AUTHOR: Marek Karaba
// DATE:   04.07.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CDialogueSkippedSignal : IEventBusSignal
	{
		public EDialogueId DialogueId { get; }
		public int CurrentDialogueCommandsIndex { get; }
		public int CurrentDialogueCommandsCount { get; }
		public int DurationInMs { get; }

		public CDialogueSkippedSignal(
			EDialogueId dialogueId,
			int currentDialogueCommandsIndex,
			int currentDialogueCommandsCount,
			int durationInMs)
		{
			DialogueId = dialogueId;
			CurrentDialogueCommandsIndex = currentDialogueCommandsIndex;
			CurrentDialogueCommandsCount = currentDialogueCommandsCount;
			DurationInMs = durationInMs;
		}
	}
}