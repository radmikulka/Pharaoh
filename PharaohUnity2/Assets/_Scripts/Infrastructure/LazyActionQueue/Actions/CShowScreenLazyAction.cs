// =========================================
// AUTHOR: Marek Karaba
// DATE:   30.10.2025
// =========================================

using System.Threading;
using AldaEngine;
using Cysharp.Threading.Tasks;

namespace Pharaoh
{
	public class CShowScreenLazyAction : ILazyAction
	{
		private readonly EScreenId _screenId;
		private readonly IEventBus _eventBus;

		public CShowScreenLazyAction(EScreenId screenId, IEventBus eventBus)
		{
			_screenId = screenId;
			_eventBus = eventBus;
		}

		public int Priority => int.MaxValue;
		
		public UniTask Execute(CancellationToken ct)
		{
			ShowScreen(_screenId);
			return UniTask.CompletedTask;
		}

		private void ShowScreen(EScreenId screenId)
		{
			_eventBus.ProcessTask(new CShowScreenTask(screenId));
		}
	}
}