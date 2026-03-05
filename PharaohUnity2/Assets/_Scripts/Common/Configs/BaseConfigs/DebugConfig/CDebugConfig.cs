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
		[SearchableEnum] [SerializeField] private ECurrencyType _currencyToDebug;
		[SearchableEnum] [SerializeField] private ELanguageCode _languageToDebug;
		[SearchableEnum] [SerializeField] [HideInInspector] private EVehicle _cheatVehicle;
		
		[Space(20)]
		[SerializeField] private EEditorSkips _editorSkips;
		[SerializeField] private ETutorialSkip _skipTutorial;
		[SerializeField] private bool _showDebugIosHomeButton = true;
		[SerializeField, OnValueChanged("OnDebugModeChanged")] private bool _debugMode = true;
		[SerializeField] private float _defaultGameSpeed = 1;

		public ELanguageCode LanguageToDebug => _languageToDebug;
		public ECurrencyType CurrencyToDebug => _currencyToDebug;
		public bool DebugMode => _debugMode;
		public bool ShowDebugIosHomeButton => _showDebugIosHomeButton;
		public float DefaultGameSpeed => _defaultGameSpeed;

		public ETutorialSkip TutorialSkip
		{
			get => _skipTutorial;
			set => _skipTutorial = value;
		}

		public EEditorSkips EditorSkips
		{
			get => _editorSkips;
			set => _editorSkips = value;
		}
		
		public Action<bool> DebugModeChanged;

#if UNITY_EDITOR
		[MenuItem("TycoonBuilder/Select/Config Debug %#d", false, 0)]
		private static void SelectSettings()
		{
			Selection.activeObject = Instance;
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Initialize()
		{
			Instance.DebugModeChanged = null;
		}
#endif

		public void PrepareDevelopmentBuild()
		{
			
		}

		public void SetTutorialTesting()
		{
			_skipTutorial = ETutorialSkip.None;
			_editorSkips = EEditorSkips.None;

			CEditorUtils.SetDirty(this);
		}

		public void SetLikeABoss()
		{
			_skipTutorial = ETutorialSkip.All;
			_editorSkips = EEditorSkips.All;

			CEditorUtils.SetDirty(this);
		}

		public bool ShouldSkip(ETutorialSkip skip)
		{
			if(!CPlatform.IsDebug)
				return false;
			
			return _skipTutorial.HasFlag(skip);
		}

		public bool ShouldSkip(EEditorSkips skip)
		{
			if(!CPlatform.IsDebug)
				return false;
			
			return _editorSkips.HasFlag(skip);
		}

		public void PrepareReleaseBuild()
		{
			_skipTutorial = ETutorialSkip.None;
			_editorSkips = EEditorSkips.None;
			_debugMode = false;
		}

		private void OnDebugModeChanged()
		{
			DebugModeChanged?.Invoke(_debugMode);
		}
	}
	
#if UNITY_EDITOR
	[CustomEditor(typeof(CDebugConfig))]
	public class CDebugConfigEditor : CBaseEditor<CDebugConfig>
	{
		private float _addXpValue = 100f;
		
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			
			EditorGUILayout.Space(30);

			if (Application.isPlaying)
			{
				DrawApplicationPlayingElements();
			}
		}


		private void DrawApplicationPlayingElements()
		{
			DrawAddXp();
			DrawCheatVehicle();
		}

		private void DrawCheatVehicle()
		{
			EditorGUILayout.Space(10);
			EditorGUILayout.LabelField("Cheat Vehicle", EditorStyles.boldLabel);
			
			SerializedProperty property = serializedObject.FindProperty("_cheatVehicle");
			EditorGUILayout.PropertyField(property);

			if (!GUILayout.Button("Add Vehicle")) 
				return;
			
			EVehicle enumValue = (EVehicle)property.intValue;
			FindAnyObjectByType<CBaseEditorCommands>().CheatVehicle(enumValue);
		}

		private void DrawAddXp()
		{
			_addXpValue = EditorGUILayout.Slider("XP amount (1-100)%", _addXpValue, 1f, 100f);
			if (GUILayout.Button("Add XP"))
			{
				FindAnyObjectByType<CBaseEditorCommands>().AddXp(_addXpValue / 100f);
			}
		}
	}
#endif
}