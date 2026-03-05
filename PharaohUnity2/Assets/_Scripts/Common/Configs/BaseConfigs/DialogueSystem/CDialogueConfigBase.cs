// =========================================
// AUTHOR: Marek Karaba
// DATE:   04.07.2025
// =========================================

using System;
using System.Collections.Generic;
using System.IO;
using AldaEngine;
using ServerData;
using UnityEngine;

namespace TycoonBuilder.Configs.Design
{
	public abstract class CDialogueConfigBase
	{
		private const string StoryDialogueTextPrefix = "StoryDialogueText";
		
		protected readonly CDialogueFactory DialogueFactory = new();

		public List<CDialogueCommand> GetDialogueConversation(EDialogueId dialogue)
		{
			return DialogueFactory.GetDialogueCommands(dialogue);
		}

		public EDialogueId FindDialogueByLangKey(string langKey)
		{
			return DialogueFactory.FindDialogueByText(langKey);
		}

		public ERegion[] GetAllRegionsUsingCharacter(EDialogueCharacter dialogueCharacter)
		{
			return DialogueFactory.GetAllEpisodesUsingCharacter(dialogueCharacter);
		}
		
		protected EDialogueId[] GetAllDialogueIdsForRegion(IBundleManager bundleManager, CResourceConfigs resourceConfigs, ERegion id)
		{
			TextAsset textAsset = LoadTextAsset(bundleManager, resourceConfigs, id);
			Dictionary<EDialogueId, List<CDialogueRow>> dialogueRows = GetDialogueRows(textAsset);
			EDialogueId[] dialogueIds = new EDialogueId[dialogueRows.Count];
			dialogueRows.Keys.CopyTo(dialogueIds, 0);
			return dialogueIds;
		}

		public static string GetFullDialogueText(string text)
		{
			return $"{StoryDialogueTextPrefix}.{text}";
		}
		
		protected void CreateDialoguesFromCsv(IBundleManager bundleManager, CResourceConfigs resourceConfigs, ERegion id)
		{
			TextAsset textAsset = LoadTextAsset(bundleManager, resourceConfigs, id);
			Dictionary<EDialogueId, List<CDialogueRow>> dialogueRows = GetDialogueRows(textAsset);
			CreateDialoguesFromRows(dialogueRows);
		}
		
		/*protected void CreateDialoguesFromCsv(IBundleManager bundleManager, CResourceConfigs resourceConfigs, ELiveEventId id)
		{
			TextAsset textAsset = LoadTextAsset(bundleManager, resourceConfigs, id);
			Dictionary<EDialogueId, List<CDialogueRow>> dialogueRows = GetDialogueRows(textAsset);
			CreateDialoguesFromRows(dialogueRows);
		}*/

		private TextAsset LoadTextAsset(IBundleManager bundleManager, CResourceConfigs resourceConfigs, ERegion id)
		{
			CDialogueFileLoader fileLoader = new ();
			return fileLoader.LoadTextAsset(bundleManager, resourceConfigs, id);
		}
		
		/*private TextAsset LoadTextAsset(IBundleManager bundleManager, CResourceConfigs resourceConfigs, ELiveEventId id)
		{
			CDialogueFileLoader fileLoader = new ();
			return fileLoader.LoadTextAsset(bundleManager, resourceConfigs, id);
		}*/

		public Dictionary<EDialogueId, List<CDialogueRow>> GetDialogueRows(TextAsset textAsset)
		{
			Dictionary<EDialogueId, List<CDialogueRow>> dialogueRows = new ();
			using StringReader stringReader = new (textAsset.text);
            
			List<CDialogueRow> currentDialogueRows = new ();
			EDialogueId currentDialogueId = EDialogueId.None;
            
			string[] separator = { ";" };
			int lineNumber = 0;
			while (stringReader.ReadLine() is { } line)
			{
				// Skipping header
				if (lineNumber == 0)
				{
					lineNumber++;
					continue;
				}
                    
				string[] columns = line.Split(separator, StringSplitOptions.None);
				
				if (columns.Length < 8)
					continue;
				
				string dialogueId = columns[0];
				if (dialogueId != "")
				{
					if (currentDialogueId != EDialogueId.None && dialogueId != currentDialogueId.ToString())
					{
						dialogueRows.Add(currentDialogueId, currentDialogueRows);
						currentDialogueRows = new List<CDialogueRow>();
					}

					EDialogueId dialogueIdEnum = (EDialogueId)Enum.Parse(typeof(EDialogueId), dialogueId);
					currentDialogueId = dialogueIdEnum;
					lineNumber = 1;
					continue;
				}

				string tags = columns[1];
				string pictureId = columns[2];
				string speaker = columns[3];
				string facialExpression = columns[4];
				string dialoguePlacement = columns[5];
				string dialogueTranslationKey = currentDialogueId.ToString() + lineNumber;
				string comment = columns[7];
                    
				EDialogueCharacter dialogueCharacterEnum = (EDialogueCharacter)Enum.Parse(typeof(EDialogueCharacter), speaker);
				Enum.TryParse(dialoguePlacement, out EDialoguePlacement dialoguePlacementEnum); // fallback to default
				Enum.TryParse(facialExpression, out ECharacterFacialExpression facialExpressionEnum); // fallback to default
				Enum.TryParse(pictureId, out EDialoguePictureId pictureIdEnum); // fallback to default

				CDialogueRow dialogueRow =
					new (currentDialogueId, tags, pictureIdEnum, dialogueCharacterEnum, facialExpressionEnum,
						dialoguePlacementEnum, dialogueTranslationKey, comment);
				currentDialogueRows.Add(dialogueRow);
                
				lineNumber++;
			}
            
			if (currentDialogueId != EDialogueId.None)
			{
				dialogueRows.Add(currentDialogueId, currentDialogueRows);
			}
			return dialogueRows;
		}

		private void CreateDialoguesFromRows(Dictionary<EDialogueId, List<CDialogueRow>> dialogueRows)
		{
			foreach (KeyValuePair<EDialogueId, List<CDialogueRow>> dialogueRow in dialogueRows)
			{
				DialogueFactory.CreateDialogue(dialogueRow.Key);
				foreach (CDialogueRow row in dialogueRow.Value)
				{
					DialogueFactory.AddDialogue(
						row.PictureId,
						row.Speaker,
						row.Line,
						row.Placement,
						row.Expression
					);
				}
			}
		}
	}
}