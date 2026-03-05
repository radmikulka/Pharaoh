// =========================================
// AUTHOR: Marek Karaba
// DATE:   08.09.2025
// =========================================

namespace TycoonBuilder.Configs.Design
{
    internal class CDialogueRowStringData
    {
        public string DialogueId { get; }
        public string Tags { get; }
        public string PictureId { get; }
        public string Speaker { get; }
        public string FacialExpression { get; }
        public string DialoguePlacement { get; }
        public string DialogueText { get; }
        public string DialogueTranslationKey { get; }
        public string Comment { get; }

        internal CDialogueRowStringData(
            string dialogueId,
            string tags,
            string pictureId,
            string speaker,
            string facialExpression,
            string dialoguePlacement,
            string dialogueText,
            string dialogueTranslationKey,
            string comment)
        {
            DialogueId = dialogueId;
            Tags = tags;
            PictureId = pictureId;
            Speaker = speaker;
            FacialExpression = facialExpression;
            DialoguePlacement = dialoguePlacement;
            DialogueText = dialogueText;
            DialogueTranslationKey = dialogueTranslationKey;
            Comment = comment;
        }
    }
}