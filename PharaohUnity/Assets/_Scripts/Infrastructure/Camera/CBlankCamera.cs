// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.09.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using KBCore.Refs;
using ServerData;
using UnityEngine;
using Zenject;

namespace Pharaoh
{
	public class CBlankCamera : ValidatedMonoBehaviour, IInitializable
	{
		[SerializeField, Self] private AudioListener _audioListener;
		[SerializeField, Self] private Camera _blankCamera;

		private IEventBus _eventBus;

		[Inject]
		private void Inject(IEventBus eventBus)
		{
			_eventBus = eventBus;
		}

		public void Initialize()
		{
			_eventBus.Subscribe<CSceneLoadedSignal>(OnSceneLoaded);
		}
		
		private void OnSceneLoaded(CSceneLoadedSignal sceneLoadedSignal)
		{
			bool isCoreGameScene = sceneLoadedSignal.Id == ESceneId.CoreGame;
			if(!isCoreGameScene)
				return;
			
			_blankCamera.enabled = false;
			_audioListener.enabled = false;
		}
	}
}