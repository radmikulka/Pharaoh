// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.09.2025
// =========================================

using AldaEngine;

namespace TycoonBuilder
{
	public class CCinematicModeHandler
	{
		private readonly CLockObject _lockObject = new("ContractCutscene");
		private readonly IInputLock _inputLock = new CInputLock("CinematicMode", EInputLockLayer.Cinematic);
		private readonly ISmartArrowLocker _smartArrowLocker;
		private readonly CEventSystem _eventSystem;
		private readonly IEventBus _eventBus;

		public CCinematicModeHandler(IEventBus eventBus, CEventSystem eventSystem, ISmartArrowLocker smartArrowLocker)
		{
			_smartArrowLocker = smartArrowLocker;
			_eventSystem = eventSystem;
			_eventBus = eventBus;
		}

		public void SetActive(bool state, bool animatedBlend)
		{
			SetActiveLetterbox(state, animatedBlend);
			SetActiveHud(!state, animatedBlend);
			SetActiveNameTags(!state);
			SetBlockInput(state);
			SetBlockSmartArrows(state);
		}
		
		private void SetBlockSmartArrows(bool state)
		{
			if (state)
			{
				_smartArrowLocker.AddLock(_lockObject);
				return;
			}
			_smartArrowLocker.RemoveLock(_lockObject);
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
		
		private void SetActiveNameTags(bool state)
		{
			if (state)
			{
				_eventBus.ProcessTask(new CRemoveFloatingWindowsBlockerTask(_lockObject));
				return;
			}
			_eventBus.ProcessTask(new CAddFloatingWindowsBlockerTask(_lockObject));
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