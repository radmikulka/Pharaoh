// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System;
using AldaEngine;
using Pharaoh;
using NaughtyAttributes;
using RoboRyanTron.SearchableEnum;
using ServerData;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Pharaoh
{
	[AssetPath("Configs/DebugConfig", "DebugConfig")]
	public class CDebugConfig : CScriptableSingleton<CDebugConfig>
	{
		[Space(20)]
		[SerializeField] private EEditorSkips _editorSkips;
		[SerializeField] private float _defaultGameSpeed = 1;

		public float DefaultGameSpeed => _defaultGameSpeed;

		public EEditorSkips EditorSkips
		{
			get => _editorSkips;
			set => _editorSkips = value;
		}

#if UNITY_EDITOR
		[MenuItem("Pharaoh/Select/Config Debug %#d", false, 0)]
		private static void SelectSettings()
		{
			Selection.activeObject = Instance;
		}
#endif

		public void PrepareDevelopmentBuild()
		{
			
		}

		public void SetTutorialTesting()
		{
			_editorSkips = EEditorSkips.None;

			CEditorUtils.SetDirty(this);
		}

		public void SetTLikeABoss()
		{
			_editorSkips = EEditorSkips.All;

			CEditorUtils.SetDirty(this);
		}

		public bool ShouldSkip(EEditorSkips skip)
		{
			if(!CPlatform.IsDebug)
				return false;
			
			return _editorSkips.HasFlag(skip);
		}

		public void PrepareReleaseBuild()
		{
			_editorSkips = EEditorSkips.None;
		}
	}
}