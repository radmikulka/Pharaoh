using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using static System.Text.RegularExpressions.Regex;

#if UNITY_EDITOR

// =========================================
// AUTHOR: Marek Karaba
// DATE:   08.09.2025
// =========================================

namespace TycoonBuilder.Configs.Design
{
	internal class CBaseEditorLocalizationUpdater : CBaseEditorTaskDialogueValidation
	{
		internal Dictionary<string, string> GetMissingTranslation(Dictionary<string, List<CDialogueRowStringData>> data)
		{
			Dictionary<string, string> localizationKeyValuePair = GetLocalizationDictionary();
			Dictionary<string, string> missingTranslations = new ();

			foreach (KeyValuePair<string, List<CDialogueRowStringData>> pair in data)
			{
				List<CDialogueRowStringData> dialogueRows = pair.Value;
                
				foreach (CDialogueRowStringData row in dialogueRows)
				{
					string translationKey = "StoryDialogueText." + row.DialogueTranslationKey;
					string translation = row.DialogueText;
                    
					if (!localizationKeyValuePair.TryGetValue(translationKey, out string currentTranslation))
					{
						missingTranslations.Add(translationKey, translation);
					}
				}
			}
			return missingTranslations;
		}
		
		internal Dictionary<string, string> GetChangedTranslation(Dictionary<string, List<CDialogueRowStringData>> data)
		{
			Dictionary<string, string> localizationKeyValuePair = GetLocalizationDictionary();
			Dictionary<string, string> changedTranslations = new ();

			foreach (KeyValuePair<string, List<CDialogueRowStringData>> pair in data)
			{
				List<CDialogueRowStringData> dialogueRows = pair.Value;
                
				foreach (CDialogueRowStringData row in dialogueRows)
				{
					string translationKey = "StoryDialogueText." + row.DialogueTranslationKey;
					string translation = row.DialogueText;
                    
					if (localizationKeyValuePair.TryGetValue(translationKey, out string currentTranslation))
					{
						if (translation != currentTranslation)
						{
							changedTranslations.Add(translationKey, translation);
						}
					}
				}
			}
			return changedTranslations;
		}
		
		internal void AddMissingTranslationToLocalization(Dictionary<string, string> missingTranslations)
		{
			Dictionary<string, string> localizationKeyValuePair = new();
			foreach (KeyValuePair<string, string> pair in missingTranslations)
			{
				localizationKeyValuePair[pair.Key] = pair.Value;
			}

			TextAsset enText = GetEnLocalizationTextAsset();
			string path = AssetDatabase.GetAssetPath(enText);

			using (StreamWriter writer = new (path, append: true))
			{
				foreach (KeyValuePair<string, string> pair in localizationKeyValuePair)
				{
					if (pair.Key == string.Empty)
					{
						Debug.LogWarning($"Key is empty: {pair.Value}");
						continue;
					}
					if (pair.Value == string.Empty)
					{
						Debug.LogWarning($"Value is empty: {pair.Key}");
						continue;
					}
                    
					string[] forbiddenCharacters = new[] { "\"", "´", "`", "“", "”", "„", "”", "“"};
					if (Array.Exists(forbiddenCharacters, element => pair.Value.Contains(element)))
					{
						Debug.LogWarning($"Forbidden characters found in line: {pair.Key}. Line won't be added.");
						continue;
					}
                    
					writer.WriteLine($"\"{pair.Key}\":\"\":\"\":\"{pair.Value}\"");
					Debug.Log($"Localization added: {pair.Key}");
				}
			}
			AssetDatabase.Refresh();
		}
		
		internal void UpdateChangedTranslationToLocalization(Dictionary<string, string> changedTranslations)
        {
            TextAsset enText = GetEnLocalizationTextAsset();
            string path = AssetDatabase.GetAssetPath(enText);

            using StringReader stringReader = new StringReader(enText.text);
            List<string> updatedLines = new List<string>();
           
            string[] separators = new[] { ":", "ALREADY DONE", "\"\"", "\"" };

            while (stringReader.ReadLine() is { } line)
            {
                string[] columns = line.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                
                if (changedTranslations.TryGetValue(columns[0], out string newValue))
                {
                    if (DoesDialogueContainAlreadyDone(columns[0]))
                    {
                        Debug.LogWarning($"Cannot update changed dialogue because ALREADY DONE: {columns[0]}");
                        updatedLines.Add(line);
                        continue;;
                    }
                   
                    if (newValue == string.Empty)
                    {
                        Debug.LogWarning($"Value is empty: {columns[0]}");
                        updatedLines.Add(line);
                        continue;
                    }
                    
                    string updatedLine = $"\"{columns[0]}\":\"\":\"\":\"{newValue}\"";

                    string[] forbiddenCharacters = new[] { "\"", "´", "`", "“", "”", "„", "”", "“"};
                    if (Array.Exists(forbiddenCharacters, element => newValue.Contains(element)))
                    {
                        Debug.LogWarning($"Forbidden characters found in line: {columns[0]}. Line won't be updated.");
                        updatedLines.Add(line);
                        continue;
                    }
                    
                    updatedLines.Add(updatedLine);
                    Debug.Log($"Localization updated: {columns[0]}");
                    continue;
                }
                updatedLines.Add(line);
            }
            
            File.WriteAllLines(path, updatedLines);
            AssetDatabase.Refresh();
        }
		
		private bool DoesDialogueContainAlreadyDone(string key)
		{
			TextAsset enText = GetEnLocalizationTextAsset();

			using StringReader stringReader = new (enText.text);
			string[] separators = { ":", "ALREADY DONE", "\"\"", "\"" };

			while (stringReader.ReadLine() is { } line)
			{
				string[] columns = line.Split(separators, StringSplitOptions.RemoveEmptyEntries);
				string localizationKey = columns[0];

				if (!localizationKey.Contains(key, StringComparison.OrdinalIgnoreCase))
					continue;
                
				if (line.Contains("ALREADY DONE"))
					return true;
			}
			return false;
		}
	}
}
#endif