// =========================================
// AUTHOR: Radek Mikulka
// DATE:   07.09.2023
// =========================================

using AldaEngine;
using NaughtyAttributes;
using RoboRyanTron.SearchableEnum;
using ServerData;
using UnityEngine;
using UnityEngine.Serialization;
using Pharaoh;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
#endif

namespace Pharaoh
{
	[AssetPath("Configs/ServerConfig", "ServerConfig")]
	public class CServerConfig : CScriptableSingleton<CServerConfig>
	{
		[Header("BASE")]
		[SerializeField] private bool _isLocal;
		[SerializeField] private EServerType _serverType;
		[SerializeField] private bool _deleteUser;
		
		[Space(5)] [Header("PRESETS")] 
		[SerializeField] private bool _likeABoss;
		[SerializeField] private string _overrideUid;
		[SerializeField] [SearchableEnum] private EUserPresetId _presetId;

		public EServerType ServerType 
		{
			get => _serverType;
			set => _serverType = value;
		}

		public bool DeleteUser 
		{
			get => _deleteUser;
			set => _deleteUser = value;
		}

		public string OverrideUid 
		{
			get => _overrideUid;
			set => _overrideUid = value;
		}

		public EUserPresetId PresetId 
		{
			get => _presetId;
			set => _presetId = value;
		}

		public bool LikeABoss
		{
			get => _likeABoss;
			set => _likeABoss = value;
		}

		public bool IsLocal => CPlatform.IsEditor && _isLocal;

		#if UNITY_EDITOR

		[Button(null, EButtonEnableMode.Playmode, 20)]
		private void SavePreset() 
		{
			FindAnyObjectByType<CBaseEditorCommands>().SavePreset();
		}

#if UNITY_EDITOR
		[MenuItem("Pharaoh/Select/Config Server %#t", false, 0)]
		private static void SelectSettings()
		{
			Selection.activeObject = Instance;
		}
		#endif
		
		public void PrepareDevelopmentBuild()
		{
			_isLocal = false;
		}

		public void PrepareReleaseBuild()
		{
			#if UNITY_EDITOR
			_isLocal = false;
			_deleteUser = false;
			_serverType = EServerType.Master;
			_likeABoss = false;
			_overrideUid = string.Empty;
			_presetId = EUserPresetId.None;
			
			if (CPlatform.IsAndroidPlatform)
			{
				PlayerSettings.SetIl2CppCompilerConfiguration(NamedBuildTarget.Android, Il2CppCompilerConfiguration.Master);
			}
			
			EditorUtility.SetDirty(this);
			#endif
		}
		#endif
	}
}