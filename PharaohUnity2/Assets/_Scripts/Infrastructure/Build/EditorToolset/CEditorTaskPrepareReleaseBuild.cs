// =========================================
// AUTHOR: Radek Mikulka
// DATE:   6.3.2024
// =========================================

#if UNITY_EDITOR
using System;
using System.IO;
using System.Threading.Tasks;
using Kamgam.ExcludeFromBuild;
using SRDebugger.Editor;
using Unity.Android.Types;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using Pharaoh;
using AndroidArchitecture = UnityEditor.AndroidArchitecture;

namespace AldaEngine
{
	[CreateAssetMenu(fileName = "PrepareReleaseBuild", menuName = "____Pharaoh/EditorTasks/Steps/BuildTools/PrepareReleaseBuild")]
	public class CEditorTaskPrepareReleaseBuild : CBaseEditorTaskStep
	{
		public override Task Execute()
		{
			CTranslationFileGenerator.GenerateRuntimeFiles();
			CEditorTaskRefreshAllBundles.RefreshBundles();
			CEditorTaskDeleteRemoteBundles.DeleteRemoteBundles();

			ExcludeFromBuildController.ApplyExcludedFileAndDirNames();

			CScriptDefineSymbols.Instance.SetActiveDebugMode(false);
			CDebugConfig.Instance.PrepareReleaseBuild();
			CServerConfig.Instance.PrepareReleaseBuild();
			CBuildConfig.Instance.PrepareReleaseBuild();

			SetProjectSettings();

			EditorUtility.SetDirty(CDebugConfig.Instance);
			EditorUtility.SetDirty(CServerConfig.Instance);
			EditorUtility.SetDirty(CAndroidKeystore.Instance);

			UnityEditor.Android.UserBuildSettings.DebugSymbols.level = DebugSymbolLevel.Full;
			EditorUserBuildSettings.buildAppBundle = true;
			EditorUserBuildSettings.development = false;
			EditorUserBuildSettings.waitForManagedDebugger = false;
			EditorUserBuildSettings.buildWithDeepProfilingSupport = false;
			EditorUserBuildSettings.connectProfiler = false;
			EditorUserBuildSettings.allowDebugging = false;

			Debug.Log($"{DisplayName} completed");
			
			SRDebugEditor.SetEnabled(false);
			return Task.CompletedTask;
		}

		private static void SetProjectSettings()
		{
			if (CPlatform.IsAndroidPlatform)
			{
				PlayerSettings.SetIl2CppCompilerConfiguration(NamedBuildTarget.Android, Il2CppCompilerConfiguration.Master);
				PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
				PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
			}
			else if (CPlatform.IsIosPlatform)
			{
				PlayerSettings.SetIl2CppCompilerConfiguration(NamedBuildTarget.iOS, Il2CppCompilerConfiguration.Master);
			}
		}
	}
}
#endif