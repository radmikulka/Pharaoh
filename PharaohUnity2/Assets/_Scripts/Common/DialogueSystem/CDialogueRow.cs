// =========================================
// AUTHOR: Marek Karaba
// DATE:   04.07.2025
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class CDialogueRow
	{
		public EDialogueId ID { get; private set; }
		public string Tags { get; private set; }
		public EDialoguePictureId PictureId { get; private set; }
		public EDialogueCharacter Speaker { get; private set; }
		public ECharacterFacialExpression Expression { get; private set; }
		public EDialoguePlacement Placement { get; private set; }
		public string Line { get; private set; }
		public string Comment { get; private set; }

		public CDialogueRow(
			EDialogueId id,
			string tags,
			EDialoguePictureId pictureId,
			EDialogueCharacter speaker,
			ECharacterFacialExpression expression,
			EDialoguePlacement placement,
			string line,
			string comment)
		{
			ID = id;
			Tags = tags;
			PictureId = pictureId;
			Speaker = speaker;
			Expression = expression;
			Placement = placement;
			Line = line;
			Comment = comment;
		}
	}
}