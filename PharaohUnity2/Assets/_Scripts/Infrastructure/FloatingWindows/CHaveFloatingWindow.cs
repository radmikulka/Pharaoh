// =========================================
// AUTHOR: Marek Karaba
// DATE:   03.07.2025
// =========================================

using System;
using AldaEngine;
using AldaEngine.AldaFramework;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TycoonBuilder
{
	public class CHaveFloatingWindow : MonoBehaviour, IConstructable, IInitializable
	{
		private IEventBus _eventBus;
		
		public IFloatingWindowOwner Owner { get; private set; }

		[Inject]
		private void Inject(IEventBus eventBus)
		{
			_eventBus = eventBus;
		}

		public void Construct()
		{
			Owner = GetComponent<IFloatingWindowOwner>();
		}

		public void Initialize()
		{
			_eventBus.Subscribe<CGameModeStartedSignal>(OnGameModeStarted);
		}

		private void OnGameModeStarted(CGameModeStartedSignal signal)
		{
			if (signal.Data.GameModeId != EGameModeId.CoreGame)
				return;
			
			//_eventBus.ProcessTask(new CSetFloatingWindowRequest(this));
		}
	}
}