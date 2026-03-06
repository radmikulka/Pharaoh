// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Pharaoh
{
	internal class CSceneLoadingTasksBuffer
	{
		private readonly Queue<Func<UniTask>> _queuedActions = new();
		private Func<UniTask> _currentTask;

		internal void Enqueue(Func<UniTask> task)
		{
			_queuedActions.Enqueue(task);
			RunAsync().Forget();
		}

		private async UniTaskVoid RunAsync()
		{
			if (_currentTask != null || _queuedActions.Count == 0)
				return;

			_currentTask = _queuedActions.Dequeue();
			await _currentTask();
			_currentTask = null;
			RunAsync().Forget();
		}
	}
}