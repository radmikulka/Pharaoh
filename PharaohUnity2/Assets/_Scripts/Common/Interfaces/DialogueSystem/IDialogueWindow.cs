// =========================================
// AUTHOR: Marek Karaba
// DATE:   04.07.2025
// =========================================

using System.Threading;
using AldaEngine;
using Cysharp.Threading.Tasks;
using ServerData;

namespace TycoonBuilder
{
	public interface IDialogueWindow : IScreen
	{
		UniTask ShowDialogue(
			EDialoguePictureId pictureId,
			EDialogueCharacter dialogueCharacterType,
			EDialoguePlacement placement,
			ECharacterFacialExpression facialExpression,
			string text,
			CancellationTokenSource cancellationTokenSource);

		void SetBlackBg(bool withBlackBg);
	}
}