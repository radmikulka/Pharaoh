// =========================================
// AUTHOR: Marek Karaba
// DATE:   04.07.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;
using ServerData;
using ServerData.Design;
using TycoonBuilder.Configs.Design;

namespace TycoonBuilder
{
	public class CDialogueFactory
	{
		private readonly Dictionary<EDialogueId, List<CDialogueCommand>> _dialogueCommands;
		private EDialogueId _currentDialogue;

		public CDialogueFactory()
		{
			_dialogueCommands = new Dictionary<EDialogueId, List<CDialogueCommand>>();
		}

		public CDialogueFactory CreateDialogue(EDialogueId dialogue)
		{
			if (!_dialogueCommands.ContainsKey(dialogue))
			{
				_dialogueCommands[dialogue] = new List<CDialogueCommand>();
			}
			_currentDialogue = dialogue;
			return this;
		}

		public CDialogueFactory AddDialogue(
			EDialoguePictureId pictureId,
			EDialogueCharacter dialogueCharacterType,
			string text,
			EDialoguePlacement placement = EDialoguePlacement.Default,
			ECharacterFacialExpression facialExpression = ECharacterFacialExpression.Default)
		{
			if (!_dialogueCommands.TryGetValue(_currentDialogue, out _))
			{
				throw new ArgumentException("Dialogue not initialized. Call AddDialogue first.");
			}

			var dialogueCommand = new DialogueCommandBuilder()
				.SetPictureId(pictureId)
				.SetCharacterType(dialogueCharacterType)
				.SetText(text)
				.SetSpecialPlacement(placement)
				.SetFacialExpression(facialExpression)
				.Build();
			_dialogueCommands[_currentDialogue].Add(dialogueCommand);

			return this;
		}
		
		public List<CDialogueCommand> GetDialogueCommands(EDialogueId dialogue)
		{
			return _dialogueCommands.TryGetValue(dialogue, out List<CDialogueCommand> command) ? command : new List<CDialogueCommand>();
		}

		public EDialogueId FindDialogueByText(string text)
		{
			foreach (var command in _dialogueCommands)
			{
				foreach (CDialogueCommand dialogueCommand in command.Value)
				{
					string fullDialogueLangKey = CDialogueConfigBase.GetFullDialogueText(dialogueCommand.Text);
					if (fullDialogueLangKey == text)
					{
						return command.Key;
					}
				}
			}
			throw new Exception($"Dialogue with text {text} not found.");
		}

		public ERegion[] GetAllEpisodesUsingCharacter(EDialogueCharacter dialogueCharacter)
		{
			CDesignRegionConfigs episodeConfigs = CDesignRegionConfigs.GetEditorInstance();
			HashSet<ERegion> result = new();
			foreach (var command in _dialogueCommands)
			{
				bool isCharacterInDialogue = command.Value.Any(c => c.DialogueCharacterType == dialogueCharacter);
				if(!isCharacterInDialogue)
					continue;
				
				/*ERegion region = episodeConfigs.GetEpisodeWithDialogueOrDefault(command.Key);
				if (region == ERegion.None)
					continue;
				
				result.Add(region);*/
			}

			return result.ToArray();
		}
	}
}