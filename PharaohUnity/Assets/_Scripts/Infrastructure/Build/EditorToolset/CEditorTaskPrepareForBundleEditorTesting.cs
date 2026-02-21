// =========================================
// AUTHOR: Radek Mikulka
// DATE:   04.07.2024
// =========================================

#if UNITY_EDITOR

using System.IO;
using System.Threading.Tasks;
using SRDebugger.Editor;
using UnityEditor;
using UnityEngine;
using Pharaoh;

namespace AldaEngine
{
	[CreateAssetMenu(fileName = "PrepareForBundleEditorTesting", menuName = "____Pharaoh/EditorTasks/Steps/Tools/PrepareForBundleEditorTesting")]
	public class CEditorTaskPrepareForBundleEditorTesting : CBaseEditorTaskStep
	{
		public override string HelpBoxContent => "Prepares the project for testing offline AssetBundles in the editor.";

		public override string DisplayName => "Prepare Editor For Bundle Testing";

		public override Task Execute()
		{
			CopyBundlesFolder();
			CEditorTaskRefreshAllBundles.RefreshBundles();
			CBuildConfig.Instance.SetBundleEditorTesting();
			Debug.Log($"{DisplayName} completed");
			return Task.CompletedTask;
		}
		
		private void CopyBundlesFolder()
		{
			string dataPath = Application.dataPath;
			DirectoryInfo projectDir = Directory.GetParent(dataPath);
			string originalBundlesPath = Path.Combine(projectDir.ToString(), "AssetBundles", "Android");
			string streamingBundlesPath = Path.Combine(Application.streamingAssetsPath, "AssetBundles", "Android");
			CEditorUtils.CreateDirectory(streamingBundlesPath);
			
			foreach (string path in Directory.GetFiles(originalBundlesPath, "*.*"))
			{
				string targetFilePath = Path.Combine(streamingBundlesPath, Path.GetFileName(path));
				File.Copy(path, targetFilePath, true);
			}
		}
	}
}
#endif