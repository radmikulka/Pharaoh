// =========================================
// AUTHOR: Juraj Joscak
// DATE:   03.09.2025
// =========================================

using System;
using System.Collections.Generic;
using AldaEngine;
using AldaEngine.AldaFramework;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CHudManager : MonoBehaviour, IInitializable, IIHaveBundleLinks
	{
		[Serializable]
		private class CHudLink
		{
			public EGameModeId Id;
			[BundleLink(true, typeof(GameObject))] public CBundleLink BundleLink;
		}
		
		[SerializeField] private CHudLink[] _hudBundleLinks;
		
		private IEventBus _eventBus;
		private CAldaInstantiator _instantiator;
		
		private readonly Dictionary<EGameModeId, GameObject> _huds = new();
		
		[Inject]
		private void Inject(IEventBus eventBus, CAldaInstantiator instantiator)
		{
			_eventBus = eventBus;
			_instantiator = instantiator;
		}

		public IEnumerable<IBundleLink> GetBundleLinks()
		{
			foreach (CHudLink link in _hudBundleLinks)
			{
				yield return link.BundleLink;
			}
		}

		public void Initialize()
		{
			_eventBus.Subscribe<CGameModeStartedSignal>(OnGameModeStarted);
		}
		
		private void OnGameModeStarted(CGameModeStartedSignal signal)
		{
			EGameModeId activeModeId = signal.Data.GameModeId;

			if (!_huds.ContainsKey(activeModeId))
			{
				CBundleLink prefabLink = GetPrefabLink(activeModeId);
				GameObject newHud = _instantiator.Instantiate(prefabLink, transform, EBundleCacheType.None);
				_huds.Add(activeModeId, newHud);
			}
			
			foreach (KeyValuePair<EGameModeId, GameObject> pair in _huds)
			{
				pair.Value.SetActiveObject(pair.Key == signal.Data.GameModeId);
			}
		}
		
		private CBundleLink GetPrefabLink(EGameModeId modeId)
		{
			foreach (CHudLink link in _hudBundleLinks)
			{
				if (link.Id == modeId)
				{
					return link.BundleLink;
				}
			}

			throw new Exception($"No HUD prefab link for mode {modeId}");
		}
	}
}