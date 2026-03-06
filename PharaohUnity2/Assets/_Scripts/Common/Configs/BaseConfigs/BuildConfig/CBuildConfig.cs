// // =========================================
// // AUTHOR: Radek Mikulka
// // DATE:   06.09.2023
// // =========================================

using System.IO;
using System.Linq;
using AldaEngine;
using DG.Tweening.Core;
using NaughtyAttributes;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Pharaoh
{
    [AssetPath("Configs/BuildConfig", "BuildConfig")]
    public class CBuildConfig : CScriptableSingleton<CBuildConfig>
    {
        [SerializeField] private string[] _buildInScenes;
        [SerializeField] private CBundleManagerSettings _bundleConfig;
        [SerializeField] private bool _showLauncherMenu;
        
        public bool ShowLauncherMenu => _showLauncherMenu;
        public CBundleManagerSettings BundleConfig => _bundleConfig;
		
#if UNITY_EDITOR
        [MenuItem("TycoonBuilder/Select/Config Build %#e", false, 0)]
        private static void SelectSettings()
        {
            Selection.activeObject = Instance;
        }
#endif
		
        public void PrepareDevelopmentBuild()
        {
            #if UNITY_EDITOR
            SetEditorBuildSettingsScenes(false, false);
            _bundleConfig.UseBundles = true;
            _bundleConfig.OfflineMode = true;
            _showLauncherMenu = true;
            EditorUtility.SetDirty(_bundleConfig);
            #endif
        }

        public void PrepareReleaseBuild()
        {
            #if UNITY_EDITOR
            SetEditorBuildSettingsScenes(false, false);
            _bundleConfig.UseBundles = true;
            _bundleConfig.OfflineMode = false;
            _showLauncherMenu = false;
            EditorUtility.SetDirty(_bundleConfig);
            #endif
        }
        
        public void SetBundleEditorTesting()
        {
            #if UNITY_EDITOR
            SetEditorBuildSettingsScenes(false, false);
            _bundleConfig.UseBundles = true;
            _bundleConfig.OfflineMode = true;
            EditorUtility.SetDirty(_bundleConfig);
            #endif
        }
        
        [Button]
        public void SetEditor()
        {
            #if UNITY_EDITOR
            SetEditorBuildSettingsScenes(true, false);
            _bundleConfig.UseBundles = false;
            EditorUtility.SetDirty(_bundleConfig);
            CScriptDefineSymbols.Instance.SetActiveDebugMode(true);
			
            DOTweenSettings doTweenSettings = FindDOTweenSettings();
            doTweenSettings.debugMode = true;
            #endif
        }
        
        #if UNITY_EDITOR
        private void SetEditorBuildSettingsScenes(bool state, bool isMasterBuild)
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            foreach (EditorBuildSettingsScene scene in scenes)
            {
                string sceneName = Path.GetFileNameWithoutExtension(scene.path);
                scene.enabled = _buildInScenes.Contains(sceneName) || state;

                if (isMasterBuild && sceneName.Equals("_Launcher"))
                {
                    scene.enabled = false;
                }
            }
            EditorBuildSettings.scenes = scenes;
        }
        
        private DOTweenSettings FindDOTweenSettings()
        {
            string[] guids = AssetDatabase.FindAssets("t:DOTweenSettings");
            if (guids.Length == 0)
            {
                Debug.LogError("DOTweenSettings not found");
                return null;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<DOTweenSettings>(path);
        }
        #endif
    }
    
    #if UNITY_EDITOR
    [CustomEditor(typeof(CBuildConfig))]
    public class CBuildConfigEditor : CBaseEditor<CBuildConfig>
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            EditorGUILayout.Space(20f);
            CEditorUtils.DrawDefaultInspector(MyTarget.BundleConfig);
        }
    }
    #endif
}