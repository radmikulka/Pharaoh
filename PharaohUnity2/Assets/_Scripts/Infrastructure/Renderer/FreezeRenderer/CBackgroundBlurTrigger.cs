// =========================================
// AUTHOR: Radek Mikulka
// DATE:   14.08.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using UnityEngine;
using Zenject;

namespace Pharaoh
{
	public class CBackgroundBlurTrigger :
		MonoBehaviour, 
		IScreenOpenStart, 
		IScreenCloseStart, 
		IAldaFrameworkComponent
	{
		private readonly CLockObject _lockObject = new("MenuBackgroundBlurTrigger");
		private IEventBus _eventBus;

		[Inject]
		private void Inject(IEventBus eventBus)
		{
			_eventBus = eventBus;
		}
		
		public void OnScreenOpenStart()
		{
			_eventBus.ProcessTask(new CActivateFrameCaptureTask(_lockObject));
		}

		public void OnScreenCloseStart()
		{
			_eventBus.ProcessTask(new CDeactivateFrameCaptureTask(_lockObject));
		}
	}
}