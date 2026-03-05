// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.09.2025
// =========================================

using AldaEngine;

namespace TycoonBuilder.GoToStates
{
	public class COpenMenuState : CGoToFsmStateWithData<EScreenId>
	{
		private EScreenId _screenId;
		
		public COpenMenuState(IEventBus eventBus) : base(eventBus)
		{
		}

		public override void SetData(EScreenId screenId)
		{
			_screenId = screenId;
		}
		
		public override void Start()
		{
			EventBus.ProcessTask(new CShowScreenTask(_screenId));
			IsCompleted = true;
		}
	}
}