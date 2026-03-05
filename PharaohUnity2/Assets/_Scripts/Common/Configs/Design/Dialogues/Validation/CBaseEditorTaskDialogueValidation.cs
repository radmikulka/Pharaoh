using System;
using System.Collections.Generic;
using System.IO;
using AldaEngine;
using ServerData;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

// =========================================
// AUTHOR: Marek Karaba
// DATE:   08.09.2025
// =========================================

namespace TycoonBuilder.Configs.Design
{
	internal class CBaseEditorTaskDialogueValidation
	{
		internal Dictionary<string, List<CDialogueRowStringData>> GetDialogueRows(TextAsset textAsset)
        {
            using StringReader stringReader = new (textAsset.text);
            
            Dictionary<string, List<CDialogueRowStringData>> dialogueRows = new ();
            List<CDialogueRowStringData> currentDialogueRows = new ();
            string currentDialogueId = "";
            
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
                string dialogueId = columns[0];
                    
                if (dialogueId != "")
                {
                    if (currentDialogueId != "" && dialogueId != currentDialogueId)
                    {
                        dialogueRows.Add(currentDialogueId, currentDialogueRows);
                        currentDialogueRows = new List<CDialogueRowStringData>();
                    }

                    currentDialogueId = dialogueId;
                    lineNumber = 1;
                    continue;
                }

                string tags = columns[1];
                string pictureId = columns[2];
                string speaker = columns[3];
                string facialExpression = columns[4];
                string dialoguePlacement = columns[5];
                string dialogueText = columns[6];
                string dialogueTranslationKey = currentDialogueId + lineNumber;
                string comment = columns[7];
                
                CDialogueRowStringData dialogueRow = new (
                    currentDialogueId, tags, pictureId, speaker, facialExpression, dialoguePlacement, dialogueText, dialogueTranslationKey, comment);
                
                currentDialogueRows.Add(dialogueRow);
                
                lineNumber++;
            }
            
            if (currentDialogueId != "")
            {
	            dialogueRows.Add(currentDialogueId, currentDialogueRows);
            }
            return dialogueRows;
        }
        
        internal void ValidateRegionId(string id, Dictionary<string, List<CDialogueRowStringData>> data)
        {
            foreach (KeyValuePair<string, List<CDialogueRowStringData>> pair in data)
            {
                string dialogueId = pair.Key;
                
                Enum.TryParse(dialogueId, out EDialogueId dialogueIdEnum);
                if (dialogueIdEnum == EDialogueId.None)
                {
                    Debug.LogError($"Dialogue ID {dialogueId} in region {id} is not valid.");
                }
            }
        }
        
        internal void ValidatePictureId(string id, Dictionary<string, List<CDialogueRowStringData>> data)
        {
            foreach (KeyValuePair<string, List<CDialogueRowStringData>> pair in data)
            {
                List<CDialogueRowStringData> dialogueRows = pair.Value;
                
                foreach (CDialogueRowStringData row in dialogueRows)
                {
                    string pictureId = row.PictureId;
                    
                    Enum.TryParse(pictureId, out EDialoguePictureId pictureIdEnum);
                    if (pictureIdEnum == EDialoguePictureId.None && pictureId != "Default" && pictureId != "")
                    {
                        Debug.LogError($"Picture ID {pictureId} in region {id} is not valid. Default will be used instead.");
                    }
                }
            }
        }
        
        internal void ValidateSpeaker(string id, Dictionary<string, List<CDialogueRowStringData>> data)
        {
            foreach (KeyValuePair<string, List<CDialogueRowStringData>> pair in data)
            {
                List<CDialogueRowStringData> dialogueRows = pair.Value;
                
                foreach (CDialogueRowStringData row in dialogueRows)
                {
                    string speaker = row.Speaker;
                    
                    Enum.TryParse(speaker, out EDialogueCharacter speakerEnum);
                    if (speakerEnum == EDialogueCharacter.None)
                    {
                        Debug.LogError($"Speaker {speaker} in region {id} is not valid.");
                    }
                }
            }
        }
        
         internal void ValidateExpression(string id, Dictionary<string, List<CDialogueRowStringData>> data)
        {
            foreach (KeyValuePair<string, List<CDialogueRowStringData>> pair in data)
            {
                List<CDialogueRowStringData> dialogueRows = pair.Value;
                
                foreach (CDialogueRowStringData row in dialogueRows)
                {
                    string expression = row.FacialExpression;
                    
                    Enum.TryParse(expression, out ECharacterFacialExpression expressionEnum);
                    if (expressionEnum == ECharacterFacialExpression.Default && expression != "Default" && expression != "")
                    {
                        Debug.LogError($"Facial expression {expression} in region {id} is not valid. Default will be used instead.");
                    }
                }
            }            
        }

        internal void ValidatePlacement(string id, Dictionary<string, List<CDialogueRowStringData>> data)
        {
            foreach (KeyValuePair<string, List<CDialogueRowStringData>> pair in data)
            {
                List<CDialogueRowStringData> dialogueRows = pair.Value;
                
                foreach (CDialogueRowStringData row in dialogueRows)
                {
                    string placement = row.DialoguePlacement;
                    
                    Enum.TryParse(placement, out EDialoguePlacement positionEnum);
                    if (positionEnum == EDialoguePlacement.Default && placement != "Default" && placement != "")
                    {
                        Debug.LogError($"Dialogue placement {placement} in region {id} is not valid. Default will be used instead.");
                    }
                }
            }
        }

        internal void ValidateTranslation(string id, Dictionary<string, List<CDialogueRowStringData>> data)
        {
            Dictionary<string, string> localizationKeyValuePair = GetLocalizationDictionary();

            foreach (KeyValuePair<string, List<CDialogueRowStringData>> pair in data)
            {
                List<CDialogueRowStringData> dialogueRows = pair.Value;
                
                foreach (CDialogueRowStringData row in dialogueRows)
                {
                    string translationKey = "StoryDialogueText." + row.DialogueTranslationKey;
                    string translation = row.DialogueText;
                    
                    if (!localizationKeyValuePair.TryGetValue(translationKey, out var currentTranslation))
                    {
                        Debug.LogError("Translation key MISSING" +
                                       $" {translationKey} in region {id} is not present in en.txt file.");
                    }
                    else
                    {
                        if (translation != currentTranslation)
                        {
                            Debug.LogError($"Translation CHANGED for key {translationKey} in region {id}. \n" +
                                           $"{currentTranslation} (Current translation) \n" +
                                           $"{translation} (New translation)");
                        }
                    }
                }
            }
        }
        
        internal Dictionary<string, string> GetLocalizationDictionary()
        {
            TextAsset enText = GetEnLocalizationTextAsset();
            using StringReader stringReader = new (enText.text);
            string[] separators = new[] {":", "ALREADY DONE", "\"\"", "\"" };
            Dictionary<string, string> localizationDictionary = new ();
            
            while (stringReader.ReadLine() is { } line)
            {
                string[] columns = line.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                if (columns.Length < 2)
                {
                    Debug.LogWarning($"Key or translation missing in line: {line}");
                    continue;
                }
                string translationKey = columns[0];
                string translationValue = columns[1];
                
                localizationDictionary.Add(translationKey, translationValue);
            }

            return localizationDictionary;
        }

        internal TextAsset GetEnLocalizationTextAsset()
        {
             const string mainLanguage = "en";
             List<TextAsset> supportedLanguagesTextAssets = GetSupportedLanguageFiles(mainLanguage);

             if (supportedLanguagesTextAssets.Count == 0)
             {
                 Debug.LogError("No supported languages found in localizations");
                 return null;
             }

             TextAsset enLocalizationFileText = supportedLanguagesTextAssets[0];
             supportedLanguagesTextAssets.RemoveAt(0);
             return enLocalizationFileText;
        }
        
        private List<TextAsset> GetSupportedLanguageFiles(string mainLanguage)
        {
            List<TextAsset> retLanguageFiles = new List<TextAsset>();
            List<string> suppLanguages = GetSupportedLanguages();

            foreach (string supportedLanguageFileName in suppLanguages)
            {
                TextAsset suppLanguageTextAsset = GetLocalizationTextAssetFromResources(supportedLanguageFileName);
                retLanguageFiles.Add(suppLanguageTextAsset);
            }

            for (int i = 0; i < retLanguageFiles.Count; i++)
            {
                TextAsset languageFile = retLanguageFiles[i];
                if (String.CompareOrdinal(languageFile.name, mainLanguage) == 0)
                {
                    retLanguageFiles[i] = retLanguageFiles[0];
                    retLanguageFiles[0] = languageFile;
                    break;
                }
            }
            return retLanguageFiles;
        }

        private List<string> GetSupportedLanguages()
        {
            const string buildFileName = "build";

            TextAsset buildTextAsset = GetLocalizationTextAssetFromResources(buildFileName);
            using StringReader sr = new (buildTextAsset.text);
            
            List<string> supportedLanguages = new List<string>();

            while (sr.ReadLine() is { } line)
            {
                supportedLanguages.Add(line);
            }

            return supportedLanguages;
        }

        private TextAsset GetLocalizationTextAssetFromResources(string fileName)
        {
            const string resourcesSubfolder = "Assets/Editor Default Resources/Localizations/";
            string fullPath = resourcesSubfolder + fileName + ".txt";
            if (File.Exists(fullPath))
            {
                TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(fullPath);
                return textAsset;
            }
			
            Debug.LogError($"Text asset file not found at: {fileName}");
            return new TextAsset();
        }
	}
}
#endif