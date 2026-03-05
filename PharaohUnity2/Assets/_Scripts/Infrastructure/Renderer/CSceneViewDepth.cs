// =========================================
// AUTHOR: Radek Mikulka
// DATE:   30.09.2025
// =========================================

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TycoonBuilder
{
	[InitializeOnLoad]
	public static class CSceneViewDepth
	{
		static CSceneViewDepth()
		{
			SceneView.duringSceneGui += OnSceneGUI;
			EditorApplication.update += Update;
		}

		private static void OnSceneGUI(SceneView sceneView)
		{
			if (sceneView != null && sceneView.camera != null)
			{
				sceneView.camera.depthTextureMode |= DepthTextureMode.Depth;
			}
		}
		
		private static void Update()
		{
			if (SceneView.lastActiveSceneView != null && SceneView.lastActiveSceneView.camera != null)
			{
				var cam = SceneView.lastActiveSceneView.camera;
				if ((cam.depthTextureMode & DepthTextureMode.Depth) == 0)
					cam.depthTextureMode |= DepthTextureMode.Depth;
			}
		}
	}
}
#endif