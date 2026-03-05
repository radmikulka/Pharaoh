// =========================================
// AUTHOR: Marek Karaba
// DATE:   04.07.2025
// =========================================

using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ServerData;
using TycoonBuilder.Configs.Design;

namespace TycoonBuilder
{
	public interface IDialogueHandler
	{
		UniTask ShowDialogue(
			EDialoguePictureId pictureId,
			EDialogueCharacter dialogueCharacterType,
			EDialoguePlacement placement,
			ECharacterFacialExpression facialExpression,
			string text,
			IWaitType waitType,
			CancellationTokenSource cancellationTokenSource);

		UniTask AddDialogue(EDialogueId dialogueId, CancellationToken cancellationToken);
		UniTask AddDialogueWithBlackBg(EDialogueId dialogueId, CancellationToken cancellationToken);
		bool IsRunning { get; }
		CDialogueConfig GetConfig();
		ERegion GetDialogueRegion(EDialogueId dialogueId);
	}
}