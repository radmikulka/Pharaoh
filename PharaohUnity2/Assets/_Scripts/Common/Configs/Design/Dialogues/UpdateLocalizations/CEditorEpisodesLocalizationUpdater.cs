using System.Collections.Generic;
using AldaEngine;
using ServerData;
using UnityEngine;

#if UNITY_EDITOR

// =========================================
// AUTHOR: Marek Karaba
// DATE:   08.09.2025
// =========================================

namespace TycoonBuilder.Configs.Design
{
    internal class CEditorEpisodesLocalizationUpdater : CBaseEditorLocalizationUpdater
	{
        internal void UpdateLocalizations()
        {
            CResourceConfigsSet<TycoonBuilder.CRegionDialogueConfig, ERegion> configsSet = CResourceConfigsEditor.Instance.RegionDialogueConfigs;
            IEnumerable<TycoonBuilder.CRegionDialogueConfig> configs = configsSet.GetConfigs();
            Dictionary<string, string> missingTranslations = new Dictionary<string, string>();
            Dictionary<string, string> changedTranslations = new Dictionary<string, string>();
            
            foreach (TycoonBuilder.CRegionDialogueConfig config in configs)
            {
                CBundleLink bundleLink = config.TextAsset;
                TextAsset textAsset = bundleLink.GetObjectInEditor<TextAsset>();
                Dictionary<string, List<CDialogueRowStringData>> data = GetDialogueRows(textAsset);

                Dictionary<string, string> missingTranslation = GetMissingTranslation(data);
                Dictionary<string, string> changedTranslation = GetChangedTranslation(data);
                
                foreach (KeyValuePair<string, string> kvp in missingTranslation)
                {
                    missingTranslations[kvp.Key] = kvp.Value;
                }

                foreach (KeyValuePair<string, string> kvp in changedTranslation)
                {
                    changedTranslations[kvp.Key] = kvp.Value;
                }
            }
            AddMissingTranslationToLocalization(missingTranslations);
            UpdateChangedTranslationToLocalization(changedTranslations);
        }
	}
}
#endif