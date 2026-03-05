// =========================================
// AUTHOR: Juraj Joščák
// DATE:   2023-09-29
// =========================================

using System.Collections.Generic;
using UnityEngine;
using System;
using AldaEngine;
using AldaEngine.AldaFramework;
using Zenject;
using Random = UnityEngine.Random;

namespace TycoonBuilder
{
	public class CKeysInput : MonoBehaviour, IConstructable
	{
		private CEventSystem _eventSystem;
		private IEventBus _eventBus;

		private readonly Vector2 _editorMovementStartPos = new(Screen.width / 2f, Screen.height / 2f);
		private bool _movementTriggered;
		private Vector2 _movementPos;

		[Inject]
		private void Inject(IEventBus eventBus, CEventSystem eventSystem)
		{
			_eventSystem = eventSystem;
			_eventBus = eventBus;
		}

		public void Construct()
		{
			ResetMovementPos();
		}

		private void Update()
		{
			UpdateEscape();
		}

		private void UpdateEscape()
		{
			if (!Input.GetKeyDown(KeyCode.Escape))
				return;
			
			bool isBackButtonBlocked = _eventSystem.IsBackButtonBlocked();
			if (isBackButtonBlocked)
			{
				_eventBus.Send(new CBlockedEscapePressedSignal());
				return;
			}
			_eventBus.Send(new CEscapePressedSignal());
		}
		
		private void ResetMovementPos()
		{
			_movementPos = _editorMovementStartPos;
		}
	}
}