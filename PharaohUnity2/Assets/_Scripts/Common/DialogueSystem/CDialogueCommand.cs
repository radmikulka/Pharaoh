// =========================================
// AUTHOR: Marek Karaba
// DATE:   04.07.2025
// =========================================

using System.Threading;
using Cysharp.Threading.Tasks;
using ServerData;

namespace TycoonBuilder
{
	public class CDialogueCommand : IDialogueCommand
	{
		private readonly EDialoguePictureId _pictureId;
		private readonly EDialoguePlacement _placement;
		private readonly ECharacterFacialExpression _facialExpression;
		private readonly IWaitType _waitType;

		public readonly EDialogueCharacter DialogueCharacterType;
		public readonly string Text;

		public CDialogueCommand(
			EDialoguePictureId pictureId,
			EDialogueCharacter dialogueCharacterType,
			EDialoguePlacement placement,
			ECharacterFacialExpression facialExpression,
			string text,
			IWaitType waitType)
		{
			_pictureId = pictureId;
			_facialExpression = facialExpression;
			DialogueCharacterType = dialogueCharacterType;
			_placement = placement;
			_waitType = waitType;
			Text = text;
		}

		public async UniTask Execute(IDialogueHandler dialogueHandler, CancellationTokenSource cancellationTokenSource)
		{
			await dialogueHandler.ShowDialogue(_pictureId, DialogueCharacterType, _placement, _facialExpression, Text, _waitType, cancellationTokenSource);
		}
	}
}