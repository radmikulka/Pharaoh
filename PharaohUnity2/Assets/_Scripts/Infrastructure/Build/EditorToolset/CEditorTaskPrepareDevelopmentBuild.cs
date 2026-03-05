// =========================================
// AUTHOR: Radek Mikulka
// DATE:   6.3.2024
// =========================================

#if UNITY_EDITOR
using System.Threading.Tasks;
using SRDebugger.Editor;
using Unity.Android.Types;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using TycoonBuilder;
using AndroidArchitecture = UnityEditor.AndroidArchitecture;

namespace AldaEngine
{
	[CreateAssetMenu(fileName = "PrepareDevelopmentBuild", menuName = "____TycoonBuilder/EditorTasks/Steps/BuildTools/PrepareDevelopmentBuild")]
	public class CEditorTaskPrepareDevelopmentBuild : CBaseEditorTaskStep
	{
		public override Task Execute()
		{
			CEditorTaskRefreshAllBundles.RefreshBundles();
			
			CScriptDefineSymbols.Instance.SetActiveDebugMode(true);
			CServerConfig.Instance.PrepareDevelopmentBuild();
			CBuildConfig.Instance.PrepareDevelopmentBuild();
			CDebugConfig.Instance.PrepareDevelopmentBuild();
			
			SetProjectSettings();
			
			EditorUtility.SetDirty(CDebugConfig.Instance);
			EditorUtility.SetDirty(CServerConfig.Instance);
			EditorUtility.SetDirty(CAndroidKeystore.Instance);

			UnityEditor.Android.UserBuildSettings.DebugSymbols.level = DebugSymbolLevel.None;
			EditorUserBuildSettings.buildAppBundle = false;
			
			Debug.Log($"{DisplayName} completed");
			SRDebugEditor.SetEnabled(true);
			return Task.CompletedTask;
		}

		private static void SetProjectSettings()
		{
			if (!CPlatform.IsAndroidPlatform) 
				return;
			
			PlayerSettings.SetIl2CppCompilerConfiguration(NamedBuildTarget.Android, Il2CppCompilerConfiguration.Debug);
			PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
			PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
		}
	}
}
#endif