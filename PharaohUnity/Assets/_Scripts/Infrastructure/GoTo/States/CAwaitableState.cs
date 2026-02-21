// =========================================
// AUTHOR: Radek Mikulka
// DATE:   08.12.2025
// =========================================

using System.Threading;
using AldaEngine;
using Cysharp.Threading.Tasks;

namespace Pharaoh.GoToStates
{
	public abstract class CAwaitableState : CGoToFsmState
	{
		private UniTask _uniTask;

		protected CAwaitableState(IEventBus eventBus) : base(eventBus)
		{
		}

		public override void Start()
		{
			base.Start();

			_uniTask = Run(CancellationToken);
		}

		protected abstract UniTask Run(CancellationToken ct);

		public override void Tick()
		{
			base.Tick();
			
			IsCompleted = _uniTask.Status != UniTaskStatus.Pending;
		}
	}
}