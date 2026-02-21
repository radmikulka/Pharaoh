// =========================================
// AUTHOR: Marek Karaba
// DATE:   23.10.2025
// =========================================

using AldaEngine;

namespace Pharaoh.GoToStates
{
	public class CKillMenuState : CGoToFsmStateWithData<EScreenId>
	{
		private EScreenId _screenId;
		
		public CKillMenuState(IEventBus eventBus) : base(eventBus)
		{
		}

		public override void SetData(EScreenId screenId)
		{
			_screenId = screenId;
		}
		
		public override void Start()
		{
			EventBus.ProcessTask(new CTryKillScreenTask(_screenId));
			IsCompleted = true;
		}
	}
}