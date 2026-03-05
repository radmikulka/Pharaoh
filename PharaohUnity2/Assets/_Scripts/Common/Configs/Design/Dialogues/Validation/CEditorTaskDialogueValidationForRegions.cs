using System;
using System.Collections.Generic;
using System.IO;
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
	internal class CEditorTaskDialogueValidationForRegions : CBaseEditorTaskDialogueValidation
	{
		internal void ValidateRegions()
		{
			CResourceConfigsSet<CRegionDialogueConfig, ERegion> configsSet = CResourceConfigsEditor.Instance.RegionDialogueConfigs;
			IEnumerable<CRegionDialogueConfig> configs = configsSet.GetConfigs();
            
			foreach (CRegionDialogueConfig config in configs)
			{
				string id = config.Id.ToString();
				CBundleLink bundleLink = config.TextAsset;
				TextAsset textAsset = bundleLink.GetObjectInEditor<TextAsset>();
				Dictionary<string, List<CDialogueRowStringData>> data = GetDialogueRows(textAsset);

				ValidateRegionId(id, data);
				ValidatePictureId(id, data);
				ValidateSpeaker(id, data);
				ValidateExpression(id, data);
				ValidatePlacement(id, data);
				ValidateTranslation(id, data);
				Debug.Log("Validation completed for episode " + id);
			}
		}
	}
}
#endif