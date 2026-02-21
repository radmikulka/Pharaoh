// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.09.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using Pharaoh.GoToStates;
using ServerData;
using Pharaoh;
using UnityEngine;
using Zenject;

namespace Pharaoh
{
	public class CGoToHandler : MonoBehaviour, IGoToHandler
	{
		private CFsmStateFactory _fsmFactory;
		private ICtsProvider _ctsProvider;
		private CEventSystem _eventSystem;
		private IEventBus _eventBus;
		private CGoToFsm _activeFsm;
		private CUser _user;
		private IGameTime _gameTime;
		
		private static readonly CInputLock InputLock = new("CGoToHandler", EInputLockLayer.Default);

		[Inject]
		private void Inject(
			CEventSystem eventSystem, 
			ICtsProvider ctsProvider, 
			DiContainer container,
			IEventBus eventBus,
			CUser user,
			IGameTime gameTime
			)
		{
			_fsmFactory = new CFsmStateFactory(container);
			_ctsProvider = ctsProvider;
			_eventSystem = eventSystem;
			_eventBus = eventBus;
			_user = user;
			_gameTime = gameTime;
		}

		private CGoToFsm GetNewFsm()
		{
			return new CGoToFsm(_ctsProvider.Token);
		}

		private void Update()
		{
			UpdateActiveFsm();
		}

		private void UpdateActiveFsm()
		{
			if(_activeFsm == null)
				return;
			
			_activeFsm.Tick();
			if (_activeFsm.IsCompleted)
			{
				StopFsm();
			}
		}

		public void TryKillActiveGoTo()
		{
			if(_activeFsm == null)
				return;
			StopFsm();
		}

		private void StartFsm(CGoToFsm fsm)
		{
			_activeFsm?.Cancel();
			
			fsm.Start();
			_activeFsm = fsm;

			if (fsm.BlockInput)
			{
				_eventSystem.AddInputLocker(InputLock);
			}
		}

		private void StopFsm()
		{
			if (_activeFsm.BlockInput)
			{
				_eventSystem.RemoveInputLocker(InputLock);
			}
			
			_activeFsm = null;
		}
	}
}