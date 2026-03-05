// =========================================
// AUTHOR: Marek Karaba
// DATE:   04.07.2025
// =========================================

using System.Collections.Generic;
using ServerData;

namespace TycoonBuilder
{
	public interface IDialogueQueue
	{
		public float EndDelayInSeconds { get; }
		public bool IsFinished { get; }
		void AddDialogueCommands(EDialogueId dialogueId, List<CDialogueCommand> dialogueCommands);
		bool CanShowDialogues();
	}
}