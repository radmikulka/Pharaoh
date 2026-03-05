// =========================================
// AUTHOR: Marek Karaba
// DATE:   24.07.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;

namespace TycoonBuilder.MenuTriggers
{
	public class CNativeRateUsHandler : IInitializable
	{
		private readonly INativeRateUs _nativeRateUs;
		private readonly IEventBus _eventBus;

		public CNativeRateUsHandler(IEventBus eventBus, INativeRateUs nativeRateUs)
		{
			_eventBus = eventBus;
			_nativeRateUs = nativeRateUs;
		}

		public void Initialize()
		{
			_eventBus.AddTaskHandler<CShowNativeRateUs>(OnNativeRateUsSignal);
		}

		private void OnNativeRateUsSignal(CShowNativeRateUs request)
		{
			_eventBus.Send(new CRateUsNativeShowSignal());
			_nativeRateUs.ShowAsync().Forget();
		}
	}
}