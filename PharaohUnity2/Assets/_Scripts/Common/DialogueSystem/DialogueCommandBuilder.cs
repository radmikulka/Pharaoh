// =========================================
// AUTHOR: Marek Karaba
// DATE:   04.07.2025
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class DialogueCommandBuilder
	{
		private EDialoguePictureId _pictureId;
		private EDialogueCharacter _dialogueCharacterType;
		private string _text;
		private EDialoguePlacement _placement = EDialoguePlacement.Default;
		private ECharacterFacialExpression _facialExpression = ECharacterFacialExpression.Default;
		private IWaitType _waitType = CWaitForInput.Instance;

		public DialogueCommandBuilder()
		{
		}
		
		public DialogueCommandBuilder SetPictureId(EDialoguePictureId pictureId)
		{
			_pictureId = pictureId;
			return this;
		}
		
		public DialogueCommandBuilder SetCharacterType(EDialogueCharacter dialogueCharacterType)
		{
			_dialogueCharacterType = dialogueCharacterType;
			return this;
		}

		public DialogueCommandBuilder SetText(string text)
		{
			_text = text;
			return this;
		}

		public DialogueCommandBuilder SetWaitType(IWaitType waitType)
		{
			_waitType = waitType;
			return this;
		}

		public DialogueCommandBuilder SetSpecialPlacement(EDialoguePlacement placement)
		{
			_placement = placement;
			return this;
		}

		public DialogueCommandBuilder SetFacialExpression(ECharacterFacialExpression facialExpression)
		{
			_facialExpression = facialExpression;
			return this;
		}

		public CDialogueCommand Build()
		{
			return new CDialogueCommand(_pictureId, _dialogueCharacterType, _placement, _facialExpression, _text, _waitType);
		}
	}
}