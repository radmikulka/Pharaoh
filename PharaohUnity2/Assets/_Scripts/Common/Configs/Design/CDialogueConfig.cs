// =========================================
// AUTHOR: Marek Karaba
// DATE:   04.07.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;
using ServerData.Design;

namespace TycoonBuilder.Configs.Design
{
	public class CDialogueConfig : CDialogueConfigBase, IAldaFrameworkComponent
    {
        private readonly CResourceConfigs _resourceConfigs;
        private readonly IBundleManager _bundleManager;
        
		private CDialogueConfig(IBundleManager bundleManager, CResourceConfigs resourceConfigs)
        {
            _bundleManager = bundleManager;
            _resourceConfigs = resourceConfigs;
		}

        public static CDialogueConfig Editor()
        {
            CDialogueConfig dialogueConfig = new (null, null);
            dialogueConfig.CreateDialogues();
            return dialogueConfig;
        }

        private void CreateDialogues()
        {
            #if UNITY_EDITOR
            CResourceConfigsSet<CRegionDialogueConfig, ERegion> regionConfigsSet = CResourceConfigsEditor.Instance.RegionDialogueConfigs;
            IEnumerable<CRegionDialogueConfig> regionConfigs = regionConfigsSet.GetConfigs();
        
            foreach (CRegionDialogueConfig config in regionConfigs)
            {
                CreateDialogue(config.Id);
            }
            
            /*CResourceConfigsSet<CLiveEventDialogueConfig, ELiveEventId> liveEventsConfigsSet = CResourceConfigsEditor.Instance.LiveEventDialogueConfigs;
            IEnumerable<CLiveEventDialogueConfig> liveEventConfigs = liveEventsConfigsSet.GetConfigs();

            foreach (CLiveEventDialogueConfig config in liveEventConfigs)
            {
                CreateDialoguesForLiveEvent(config.Id);
            }*/
            #endif
        }

        public static CDialogueConfig Runtime(IBundleManager bundleManager, CResourceConfigs resourceConfigs)
        {
            return new CDialogueConfig(bundleManager, resourceConfigs);
        }

        private void CreateDialogue(ERegion regionId)
        {
            CreateDialoguesFromCsv(_bundleManager, _resourceConfigs, regionId);
        }

        public void TryLoadDialogueFile(ERegion regionId)
        {
            CreateDialogue(regionId);
        }
        
        /* private void CreateDialoguesForLiveEvent(ELiveEventId liveEventId)
        {
           CreateDialoguesFromCsv(_bundleManager, _resourceConfigs, liveEventId);
        }*/

        /* private bool TryCreateLiveEventDialogue(EDialogueId dialogueId)
        {
            foreach (CLiveEventDialogueConfig config in _resourceConfigs.LiveEventDialogueConfigs.GetConfigs())
            {
                if (config.DialoguesTasks.Any(taskConfig => taskConfig.DialogueId == dialogueId))
                {
                    CreateDialoguesForLiveEvent(config.Id);
                    return true;
                }
            }
            return false;
        }*/
        public EDialogueId[] GetAllDialogueIdsForRegion(ERegion id)
        {
            EDialogueId[] dialogueIds =  GetAllDialogueIdsForRegion(_bundleManager, _resourceConfigs, id);
            return dialogueIds;
        }
    }
}