// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.10.2023
// =========================================

using System;
using System.Collections.Generic;
using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Pharaoh
{
	public class CLazyActionQueue : MonoBehaviour, IAldaFrameworkComponent
	{
		private readonly List<ILazyAction> _actionsToExecute = new();
		private readonly HashSet<CLockObject> _lockObjects = new();

		private ICtsProvider _ctsProvider;
		private IEventBus _eventBus;
		private bool _isActivated;

		public bool IsProcessing { get; private set; }

		[Inject]
		private void Inject(ICtsProvider ctsProvider, IEventBus eventBus)
		{
			_ctsProvider = ctsProvider;
			_eventBus = eventBus;
		}

		public void Activate()
		{
			_isActivated = true;
		}

		public void AddAction(ILazyAction action)
		{
			_actionsToExecute.Add(action);
		}
		
		public void AddLockObject(CLockObject lockObject)
		{
			_lockObjects.Add(lockObject);
		}

		public void RemoveLockObject(CLockObject lockObject)
		{
			_lockObjects.Remove(lockObject);
			Tick();
		}

		private void Update()
		{
			Tick();
		}

		private void Tick()
		{
			bool canProcess = CanProcess();
			if (canProcess)
			{
				CancellationToken ct = _ctsProvider.Token;
				ProcessQueuedActions(ct).Forget();
			}
		}

		private bool CanProcess()
		{
			if (!_isActivated)
				return false;
			
			if (_lockObjects.Count > 0)
				return false;
			
			if (IsProcessing)
				return false;
			
			if (_actionsToExecute.Count == 0)
				return false;

			if (_ctsProvider.Token.IsCancellationRequested)
				return false;

			bool isAnyMenuOpened = IsAnyMenuOpened();
			if (isAnyMenuOpened)
				return false;

			return true;
		}

		private bool IsAnyMenuOpened()
		{
			CMenuManagerStateResponse response = _eventBus.ProcessTask<CMenuManagerStateRequest, CMenuManagerStateResponse>();
			if (response == null)
				return false;

			return response.Active;
		}

		private async UniTask ProcessQueuedActions(CancellationToken ct)
		{
			_actionsToExecute.Sort((a, b) => b.Priority.CompareTo(a.Priority));
			IsProcessing = true;
			try
			{
				await _actionsToExecute[0].Execute(ct);
			}
			finally
			{
				IsProcessing = false;
				_actionsToExecute.RemoveAt(0);
			}
		}
	}
}