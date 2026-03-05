// =========================================
// AUTHOR: Marek Karaba
// DATE:   04.07.2025
// =========================================

using System;
using AldaEngine;
using ServerData;
using UnityEngine;

namespace TycoonBuilder.Configs.Design
{
	public class CDialogueFileLoader
	{
		public TextAsset LoadTextAsset(IBundleManager bundleManager, CResourceConfigs resourceConfigs, ERegion id)
		{
			TextAsset textAsset;
			if (bundleManager != null && resourceConfigs != null)
			{
				textAsset = LoadTextAssetRuntime(bundleManager, resourceConfigs, id);
			}
			else
			{
				textAsset = LoadTextAssetEditor(id);
			}
			return textAsset;
		}
        
		/*public TextAsset LoadTextAsset(IBundleManager bundleManager, CResourceConfigs resourceConfigs, ELiveEventId id)
		{
			TextAsset textAsset;
			if (bundleManager != null && resourceConfigs != null)
			{
				textAsset = LoadTextAssetRuntime(bundleManager, resourceConfigs, id);
			}
			else
			{
				textAsset = LoadTextAssetEditor(id);
			}
			return textAsset;
		}*/
		
		private TextAsset LoadTextAssetRuntime(IBundleManager bundleManager, CResourceConfigs resourceConfigs, ERegion id)
		{
			if (bundleManager != null && resourceConfigs != null)
			{
				CRegionDialogueConfig config = resourceConfigs.RegionDialogueConfigs.GetConfig(id);
				TextAsset textAsset = bundleManager.LoadItem<TextAsset>(config.TextAsset, EBundleCacheType.Persistent);
				return textAsset;
			}
			
			throw new Exception($"TextAsset not found for dialogue config.");
		}
        
		/*private TextAsset LoadTextAssetRuntime(IBundleManager bundleManager, CResourceConfigs resourceConfigs, ELiveEventId id)
		{
			if (bundleManager != null && resourceConfigs != null)
			{
				CLiveEventDialogueConfig config = resourceConfigs.LiveEventDialogueConfigs.GetConfig(id);
				return bundleManager.LoadItem<TextAsset>(config.TextAsset, EBundleCacheType.Persistent);
			}
			
			throw new Exception($"TextAsset not found for dialogue config.");
		}*/
		
		private TextAsset LoadTextAssetEditor(ERegion id)
		{
			#if UNITY_EDITOR
			TycoonBuilder.CRegionDialogueConfig cfg = CResourceConfigsEditor.Instance.RegionDialogueConfigs.GetConfig(id);
			CBundleLink bundleLink = cfg.TextAsset;
			return bundleLink.GetObjectInEditor<TextAsset>();
			#endif
			throw new Exception($"TextAsset not found for dialogue config.");
		}
        
		/*private TextAsset LoadTextAssetEditor(ELiveEventId id)
		{
			#if UNITY_EDITOR
			CLiveEventDialogueConfig cfg = CResourceConfigsEditor.Instance.LiveEventDialogueConfigs.GetConfig(id);
			CBundleLink bundleLink = cfg.TextAsset;
			return bundleLink.GetObjectInEditor<TextAsset>();
			#endif
			throw new Exception($"TextAsset not found for dialogue config.");
		}*/
	}
}