// =========================================
// AUTHOR: Marek Karaba
// DATE:   24.07.2025
// =========================================

using System;
using AldaEngine;
using AldaEngine.AldaFramework;

namespace TycoonBuilder.MenuTriggers
{
	public class CNativeRateUsHandlerEditor : IInitializable
	{
		private readonly IEventBus _eventBus;

		public CNativeRateUsHandlerEditor(IEventBus eventBus)
		{
			_eventBus = eventBus;
		}

		public void Initialize()
		{
			_eventBus.AddTaskHandler<CShowNativeRateUsEditor>(OnNativeRatesUsEditorSignal);
		}

		private void OnNativeRatesUsEditorSignal(CShowNativeRateUsEditor task)
		{
			_eventBus.Send(new CRateUsNativeShowSignal());
			_eventBus.ProcessTask(new CShowScreenTask(EScreenId.NativeRateUsEditor));
		}
	}
}