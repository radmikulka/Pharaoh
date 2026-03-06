// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System;
using AldaEngine;
using TycoonBuilder;
using NaughtyAttributes;
using RoboRyanTron.SearchableEnum;
using ServerData;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace TycoonBuilder
{
	[AssetPath("Configs/DebugConfig", "DebugConfig")]
	public class CDebugConfig : CScriptableSingleton<CDebugConfig>
	{
		[SerializeField] private float _defaultGameSpeed = 1;

		public float DefaultGameSpeed => _defaultGameSpeed;

#if UNITY_EDITOR
		[MenuItem("TycoonBuilder/Select/Config Debug %#d", false, 0)]
		private static void SelectSettings()
		{
			Selection.activeObject = Instance;
		}
#endif

		public void PrepareDevelopmentBuild()
		{
			
		}

		public void PrepareReleaseBuild()
		{
			
		}
	}
}