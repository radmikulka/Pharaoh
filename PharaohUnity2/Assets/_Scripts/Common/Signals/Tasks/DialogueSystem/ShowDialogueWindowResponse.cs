// =========================================
// AUTHOR: Marek Karaba
// DATE:   04.07.2025
// =========================================

namespace TycoonBuilder
{
	public class ShowDialogueWindowResponse
	{
		public IDialogueWindow DialogueWindow { get; }

		public ShowDialogueWindowResponse(IDialogueWindow dialogueWindow)
		{
			DialogueWindow = dialogueWindow;
		}
	}
}