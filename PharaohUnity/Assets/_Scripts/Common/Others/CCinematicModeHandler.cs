// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.09.2025
// =========================================

using AldaEngine;

namespace Pharaoh
{
	public class CCinematicModeHandler
	{
		private readonly CLockObject _lockObject = new("ContractCutscene");
		private readonly IInputLock _inputLock = new CInputLock("CinematicMode", EInputLockLayer.Cinematic);
		private readonly CEventSystem _eventSystem;
		private readonly IEventBus _eventBus;

		public CCinematicModeHandler(IEventBus eventBus, CEventSystem eventSystem)
		{
			_eventSystem = eventSystem;
			_eventBus = eventBus;
		}

		public void SetActive(bool state, bool animatedBlend)
		{
			SetActiveLetterbox(state, animatedBlend);
			SetActiveHud(!state, animatedBlend);
			SetBlockInput(state);
		}

		private void SetBlockInput(bool state)
		{
			if (state)
			{
				_eventSystem.AddInputLocker(_inputLock);
				return;
			}
			_eventSystem.RemoveInputLocker(_inputLock);
		}

		private void SetActiveHud(bool state, bool animatedBlend)
		{
			if (state)
			{
				_eventBus.ProcessTask(new CHudShowTask(_lockObject, !animatedBlend, true));
				return;
			}
			_eventBus.ProcessTask(new CHudHideTask(_lockObject, !animatedBlend, true));
		}
		
		private void SetActiveLetterbox(bool state, bool animatedBlend)
		{
			if (state)
			{
				_eventBus.ProcessTask(new CSetActiveLetterboxTask(true, animatedBlend));
				return;
			}
			
			_eventBus.ProcessTask(new CSetActiveLetterboxTask(false, animatedBlend));
		}
	}
}