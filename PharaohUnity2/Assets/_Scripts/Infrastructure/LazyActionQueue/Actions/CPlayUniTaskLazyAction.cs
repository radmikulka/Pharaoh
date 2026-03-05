// =========================================
// AUTHOR: Marek Karaba
// DATE:   30.10.2025
// =========================================

using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace TycoonBuilder
{
	public class CPlayUniTaskLazyAction : ILazyAction
	{
		private readonly Func<CancellationToken, UniTask> _taskFactory;

		public CPlayUniTaskLazyAction(Func<CancellationToken, UniTask> taskFactory)
		{
			_taskFactory = taskFactory;
		}

		public int Priority => int.MaxValue;

		public async UniTask Execute(CancellationToken ct)
		{
			await _taskFactory(ct);
		}
	}
}