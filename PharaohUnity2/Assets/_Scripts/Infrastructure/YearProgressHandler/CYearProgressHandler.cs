// =========================================
// AUTHOR: Marek Karaba
// DATE:   14.08.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;

namespace TycoonBuilder
{
	public class CYearProgressHandler : IInitializable
	{
		private readonly CLazyActionQueue _lazyActionQueue;
		private readonly IEventBus _eventBus;
		private readonly CUser _user;

		public CYearProgressHandler(CLazyActionQueue lazyActionQueue, IEventBus eventBus, CUser user)
		{
			_lazyActionQueue = lazyActionQueue;
			_eventBus = eventBus;
			_user = user;
		}

		public void Initialize()
		{
			_eventBus.Subscribe<CYearIncreasedSignal>(OnYearIncreased);
			TryRunOnLoad();
		}

		private void OnYearIncreased(CYearIncreasedSignal signal)
		{
			RunYearClaim();
		}

		private void TryRunOnLoad()
		{
			if(_user.Progress.Year == _user.Progress.SeenYear)
				return;
			RunYearClaim();
		}

		private void RunYearClaim()
		{
			_lazyActionQueue.AddAction(new CShowScreenLazyAction(EScreenId.NewYear, _eventBus));
		}
	}
}