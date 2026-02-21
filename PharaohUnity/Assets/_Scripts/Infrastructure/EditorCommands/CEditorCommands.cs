// =========================================
// AUTHOR: Radek Mikulka
// DATE:   2023-09-07
// =========================================

using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using ServerData;
using ServerData.Dto;
using ServerData.Hits;
using Pharaoh;
using UnityEditor;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Pharaoh
{
	public class CEditorCommands : CBaseEditorCommands
	{
		private ISceneManager _sceneManager;
		private CHitBuilder _hitBuilder;
		private IEventBus _eventBus;

		[Inject]
		private void Inject(
			ISceneManager sceneManager, 
			IGameTime gameTime, 
			CHitBuilder hitBuilder, 
			IEventBus eventBus,
			IMapper mapper,
			CUser user
			)
		{
			_sceneManager = sceneManager;
			_hitBuilder = hitBuilder;
			_eventBus = eventBus;
		}

		public override void SavePreset()
		{
			#if UNITY_EDITOR
			string presetName = GetPresetName();
			if (EditorUtility.DisplayDialog("Presset", $"Určitě? Přepíše se preset {presetName}", "Ano", "Ne"))
			{
				CSavePresetRequest hit = new(presetName);
				CHitRecordBuilder builder = _hitBuilder.GetBuilder(hit)
					.SetSendAsSingleHit();
				_hitBuilder.BuildAndSend(builder);
			}
			#endif
		}

		[Button]
		public override void RunMemoryDebug()
		{
			_eventBus.Send(new CGameResetSignal(null));
			_sceneManager.ResetGame();
			Resources.UnloadUnusedAssets();
			GC.Collect();
		}

		#if UNITY_EDITOR
		private string GetPresetName()
		{
			return CServerConfig.Instance.PresetId.ToString();
		}
		#endif

	}
}