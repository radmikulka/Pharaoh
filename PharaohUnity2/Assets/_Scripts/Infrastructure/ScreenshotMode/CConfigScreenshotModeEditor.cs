// =========================================
// AUTHOR: Marek Karaba
// DATE:   03.07.2025
// =========================================

using AldaEngine;
using UnityEditor;
using UnityEngine;

namespace TycoonBuilder
{
#if UNITY_EDITOR
	[CustomEditor(typeof(CScreenshotModeConfig))]
	public class CConfigScreenshotModeEditor : CBaseEditor<CScreenshotModeConfig>
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			
			if (!Application.isPlaying)
			{
				EditorGUI.BeginDisabledGroup(true);
				GUILayout.Box("Only Available in Play mode");
				EditorGUI.EndDisabledGroup();
				return;
			}

			GUILayout.Space(10);
			GUILayout.Label("Actions", EditorStyles.boldLabel);

			if (GUILayout.Button("Apply Changes"))
			{
				CScreenshotModeConfig.Instance.ToggleEScreenshotModeObjects();
			}
			if (GUILayout.Button("Reset Changes"))
			{
				CScreenshotModeConfig.Instance.ResetCanvasChanges();
			}
			Repaint();
		}
	}
#endif
}
